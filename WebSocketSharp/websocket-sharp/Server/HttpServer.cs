#region License
/*
 * HttpServer.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2024 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

#region Contributors
/*
 * Contributors:
 * - Juan Manuel Lallana <juan.manuel.lallana@gmail.com>
 * - Liryna <liryna.stark@gmail.com>
 * - Rohan Singh <rohan-singh@hotmail.com>
 */
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;
using System.Threading;
using WebSocketSharp.Net;
using WebSocketSharp.Net.WebSockets;

namespace WebSocketSharp.Server
{
  /// <summary>
  /// Provides a simple HTTP server.
  /// </summary>
  /// <remarks>
  ///   <para>
  ///   The server supports HTTP/1.1 version request and response.
  ///   </para>
  ///   <para>
  ///   Also the server allows to accept WebSocket handshake requests.
  ///   </para>
  ///   <para>
  ///   This class can provide multiple WebSocket services.
  ///   </para>
  /// </remarks>
  public class HttpServer
  {
    #region Private Fields

    private System.Net.IPAddress    _address;
    private string                  _docRootPath;
    private bool                    _isSecure;
    private HttpListener            _listener;
    private Logger                  _log;
    private int                     _port;
    private Thread                  _receiveThread;
    private WebSocketServiceManager _services;
    private volatile ServerState    _state;
    private object                  _sync;

    #endregion

    #region Public Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class.
    /// </summary>
    /// <remarks>
    /// The new instance listens for incoming requests on
    /// <see cref="System.Net.IPAddress.Any"/> and port 80.
    /// </remarks>
    public HttpServer ()
    {
      init ("*", System.Net.IPAddress.Any, 80, false);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class with
    /// the specified port.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   The new instance listens for incoming requests on
    ///   <see cref="System.Net.IPAddress.Any"/> and <paramref name="port"/>.
    ///   </para>
    ///   <para>
    ///   It provides secure connections if <paramref name="port"/> is 443.
    ///   </para>
    /// </remarks>
    /// <param name="port">
    /// An <see cref="int"/> that specifies the number of the port on which
    /// to listen.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="port"/> is less than 1 or greater than 65535.
    /// </exception>
    public HttpServer (int port)
      : this (port, port == 443)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class with
    /// the specified URL.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   The new instance listens for incoming requests on the IP address and
    ///   port of <paramref name="url"/>.
    ///   </para>
    ///   <para>
    ///   Either port 80 or 443 is used if <paramref name="url"/> includes
    ///   no port. Port 443 is used if the scheme of <paramref name="url"/>
    ///   is https; otherwise, port 80 is used.
    ///   </para>
    ///   <para>
    ///   The new instance provides secure connections if the scheme of
    ///   <paramref name="url"/> is https.
    ///   </para>
    /// </remarks>
    /// <param name="url">
    /// A <see cref="string"/> that specifies the HTTP URL of the server.
    /// </param>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="url"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="url"/> is invalid.
    ///   </para>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="url"/> is <see langword="null"/>.
    /// </exception>
    public HttpServer (string url)
    {
      if (url == null)
        throw new ArgumentNullException ("url");

      if (url.Length == 0)
        throw new ArgumentException ("An empty string.", "url");

      Uri uri;
      string msg;

      if (!tryCreateUri (url, out uri, out msg))
        throw new ArgumentException (msg, "url");

      var host = uri.GetDnsSafeHost (true);
      var addr = host.ToIPAddress ();

      if (addr == null) {
        msg = "The host part could not be converted to an IP address.";

        throw new ArgumentException (msg, "url");
      }

      if (!addr.IsLocal ()) {
        msg = "The IP address of the host is not a local IP address.";

        throw new ArgumentException (msg, "url");
      }

      init (host, addr, uri.Port, uri.Scheme == "https");
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class with
    /// the specified port and boolean if secure or not.
    /// </summary>
    /// <remarks>
    /// The new instance listens for incoming requests on
    /// <see cref="System.Net.IPAddress.Any"/> and <paramref name="port"/>.
    /// </remarks>
    /// <param name="port">
    /// An <see cref="int"/> that specifies the number of the port on which
    /// to listen.
    /// </param>
    /// <param name="secure">
    /// A <see cref="bool"/>: <c>true</c> if the new instance provides
    /// secure connections; otherwise, <c>false</c>.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="port"/> is less than 1 or greater than 65535.
    /// </exception>
    public HttpServer (int port, bool secure)
    {
      if (!port.IsPortNumber ()) {
        var msg = "Less than 1 or greater than 65535.";

        throw new ArgumentOutOfRangeException ("port", msg);
      }

      init ("*", System.Net.IPAddress.Any, port, secure);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class with
    /// the specified IP address and port.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   The new instance listens for incoming requests on
    ///   <paramref name="address"/> and <paramref name="port"/>.
    ///   </para>
    ///   <para>
    ///   It provides secure connections if <paramref name="port"/> is 443.
    ///   </para>
    /// </remarks>
    /// <param name="address">
    /// A <see cref="System.Net.IPAddress"/> that specifies the local IP
    /// address on which to listen.
    /// </param>
    /// <param name="port">
    /// An <see cref="int"/> that specifies the number of the port on which
    /// to listen.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="address"/> is not a local IP address.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="address"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="port"/> is less than 1 or greater than 65535.
    /// </exception>
    public HttpServer (System.Net.IPAddress address, int port)
      : this (address, port, port == 443)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="HttpServer"/> class with
    /// the specified IP address, port, and boolean if secure or not.
    /// </summary>
    /// <remarks>
    /// The new instance listens for incoming requests on
    /// <paramref name="address"/> and <paramref name="port"/>.
    /// </remarks>
    /// <param name="address">
    /// A <see cref="System.Net.IPAddress"/> that specifies the local IP
    /// address on which to listen.
    /// </param>
    /// <param name="port">
    /// An <see cref="int"/> that specifies the number of the port on which
    /// to listen.
    /// </param>
    /// <param name="secure">
    /// A <see cref="bool"/>: <c>true</c> if the new instance provides
    /// secure connections; otherwise, <c>false</c>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// <paramref name="address"/> is not a local IP address.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="address"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="port"/> is less than 1 or greater than 65535.
    /// </exception>
    public HttpServer (System.Net.IPAddress address, int port, bool secure)
    {
      if (address == null)
        throw new ArgumentNullException ("address");

      if (!address.IsLocal ()) {
        var msg = "Not a local IP address.";

        throw new ArgumentException (msg, "address");
      }

      if (!port.IsPortNumber ()) {
        var msg = "Less than 1 or greater than 65535.";

        throw new ArgumentOutOfRangeException ("port", msg);
      }

      init (address.ToString (true), address, port, secure);
    }

    #endregion

    #region Public Properties

    /// <summary>
    /// Gets the IP address of the server.
    /// </summary>
    /// <value>
    /// A <see cref="System.Net.IPAddress"/> that represents the local IP
    /// address on which to listen for incoming requests.
    /// </value>
    public System.Net.IPAddress Address {
      get {
        return _address;
      }
    }

    /// <summary>
    /// Gets or sets the scheme used to authenticate the clients.
    /// </summary>
    /// <remarks>
    /// The set operation works if the current state of the server is
    /// Ready or Stop.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   One of the <see cref="WebSocketSharp.Net.AuthenticationSchemes"/>
    ///   enum values.
    ///   </para>
    ///   <para>
    ///   It represents the scheme used to authenticate the clients.
    ///   </para>
    ///   <para>
    ///   The default value is
    ///   <see cref="WebSocketSharp.Net.AuthenticationSchemes.Anonymous"/>.
    ///   </para>
    /// </value>
    public AuthenticationSchemes AuthenticationSchemes {
      get {
        return _listener.AuthenticationSchemes;
      }

      set {
        lock (_sync) {
          if (!canSet ())
            return;

          _listener.AuthenticationSchemes = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the path to the document folder of the server.
    /// </summary>
    /// <remarks>
    /// The set operation works if the current state of the server is
    /// Ready or Stop.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   A <see cref="string"/> that represents a path to the folder
    ///   from which to find the requested file.
    ///   </para>
    ///   <para>
    ///   / or \ is trimmed from the end of the value if present.
    ///   </para>
    ///   <para>
    ///   The default value is "./Public".
    ///   </para>
    /// </value>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   The value specified for a set operation is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   The value specified for a set operation is an absolute root.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   The value specified for a set operation is an invalid path string.
    ///   </para>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// The value specified for a set operation is <see langword="null"/>.
    /// </exception>
    public string DocumentRootPath {
      get {
        return _docRootPath;
      }

      set {
        if (value == null)
          throw new ArgumentNullException ("value");

        if (value.Length == 0)
          throw new ArgumentException ("An empty string.", "value");

        value = value.TrimSlashOrBackslashFromEnd ();

        if (value == "/")
          throw new ArgumentException ("An absolute root.", "value");

        if (value == "\\")
          throw new ArgumentException ("An absolute root.", "value");

        if (value.Length == 2 && value[1] == ':')
          throw new ArgumentException ("An absolute root.", "value");

        string full = null;

        try {
          full = Path.GetFullPath (value);
        }
        catch (Exception ex) {
          throw new ArgumentException ("An invalid path string.", "value", ex);
        }

        if (full == "/")
          throw new ArgumentException ("An absolute root.", "value");

        full = full.TrimSlashOrBackslashFromEnd ();

        if (full.Length == 2 && full[1] == ':')
          throw new ArgumentException ("An absolute root.", "value");

        lock (_sync) {
          if (!canSet ())
            return;

          _docRootPath = value;
        }
      }
    }

    /// <summary>
    /// Gets a value indicating whether the server has started.
    /// </summary>
    /// <value>
    /// <c>true</c> if the server has started; otherwise, <c>false</c>.
    /// </value>
    public bool IsListening {
      get {
        return _state == ServerState.Start;
      }
    }

    /// <summary>
    /// Gets a value indicating whether the server provides secure connections.
    /// </summary>
    /// <value>
    /// <c>true</c> if the server provides secure connections; otherwise,
    /// <c>false</c>.
    /// </value>
    public bool IsSecure {
      get {
        return _isSecure;
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the server cleans up
    /// the inactive sessions periodically.
    /// </summary>
    /// <remarks>
    /// The set operation works if the current state of the server is
    /// Ready or Stop.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   <c>true</c> if the server cleans up the inactive sessions
    ///   every 60 seconds; otherwise, <c>false</c>.
    ///   </para>
    ///   <para>
    ///   The default value is <c>true</c>.
    ///   </para>
    /// </value>
    public bool KeepClean {
      get {
        return _services.KeepClean;
      }

      set {
        _services.KeepClean = value;
      }
    }

    /// <summary>
    /// Gets the logging function for the server.
    /// </summary>
    /// <remarks>
    /// The default logging level is <see cref="LogLevel.Error"/>.
    /// </remarks>
    /// <value>
    /// A <see cref="Logger"/> that provides the logging function.
    /// </value>
    public Logger Log {
      get {
        return _log;
      }
    }

    /// <summary>
    /// Gets the port of the server.
    /// </summary>
    /// <value>
    /// An <see cref="int"/> that represents the number of the port on which
    /// to listen for incoming requests.
    /// </value>
    public int Port {
      get {
        return _port;
      }
    }

    /// <summary>
    /// Gets or sets the name of the realm associated with the server.
    /// </summary>
    /// <remarks>
    /// The set operation works if the current state of the server is
    /// Ready or Stop.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   A <see cref="string"/> that represents the name of the realm.
    ///   </para>
    ///   <para>
    ///   "SECRET AREA" is used as the name of the realm if the value is
    ///   <see langword="null"/> or an empty string.
    ///   </para>
    ///   <para>
    ///   The default value is <see langword="null"/>.
    ///   </para>
    /// </value>
    public string Realm {
      get {
        return _listener.Realm;
      }

      set {
        lock (_sync) {
          if (!canSet ())
            return;

          _listener.Realm = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the server is allowed to
    /// be bound to an address that is already in use.
    /// </summary>
    /// <remarks>
    ///   <para>
    ///   You should set this property to <c>true</c> if you would like to
    ///   resolve to wait for socket in TIME_WAIT state.
    ///   </para>
    ///   <para>
    ///   The set operation works if the current state of the server is
    ///   Ready or Stop.
    ///   </para>
    /// </remarks>
    /// <value>
    ///   <para>
    ///   <c>true</c> if the server is allowed to be bound to an address
    ///   that is already in use; otherwise, <c>false</c>.
    ///   </para>
    ///   <para>
    ///   The default value is <c>false</c>.
    ///   </para>
    /// </value>
    public bool ReuseAddress {
      get {
        return _listener.ReuseAddress;
      }

      set {
        lock (_sync) {
          if (!canSet ())
            return;

          _listener.ReuseAddress = value;
        }
      }
    }

    /// <summary>
    /// Gets the configuration for secure connection.
    /// </summary>
    /// <remarks>
    /// The configuration is used when the server attempts to start,
    /// so it must be configured before the start method is called.
    /// </remarks>
    /// <value>
    /// A <see cref="ServerSslConfiguration"/> that represents the
    /// configuration used to provide secure connections.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// The server does not provide secure connections.
    /// </exception>
    public ServerSslConfiguration SslConfiguration {
      get {
        if (!_isSecure) {
          var msg = "The server does not provide secure connections.";

          throw new InvalidOperationException (msg);
        }

        return _listener.SslConfiguration;
      }
    }

    /// <summary>
    /// Gets or sets the delegate called to find the credentials for
    /// an identity used to authenticate a client.
    /// </summary>
    /// <remarks>
    /// The set operation works if the current state of the server is
    /// Ready or Stop.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   A <see cref="T:System.Func{IIdentity, NetworkCredential}"/>
    ///   delegate.
    ///   </para>
    ///   <para>
    ///   It represents the delegate called when the server finds
    ///   the credentials used to authenticate a client.
    ///   </para>
    ///   <para>
    ///   It must return <see langword="null"/> if the credentials
    ///   are not found.
    ///   </para>
    ///   <para>
    ///   <see langword="null"/> if not necessary.
    ///   </para>
    ///   <para>
    ///   The default value is <see langword="null"/>.
    ///   </para>
    /// </value>
    public Func<IIdentity, NetworkCredential> UserCredentialsFinder {
      get {
        return _listener.UserCredentialsFinder;
      }

      set {
        lock (_sync) {
          if (!canSet ())
            return;

          _listener.UserCredentialsFinder = value;
        }
      }
    }

    /// <summary>
    /// Gets or sets the time to wait for the response to the WebSocket
    /// Ping or Close.
    /// </summary>
    /// <remarks>
    /// The set operation works if the current state of the server is
    /// Ready or Stop.
    /// </remarks>
    /// <value>
    ///   <para>
    ///   A <see cref="TimeSpan"/> that represents the time to wait for
    ///   the response.
    ///   </para>
    ///   <para>
    ///   The default value is the same as 1 second.
    ///   </para>
    /// </value>
    /// <exception cref="ArgumentOutOfRangeException">
    /// The value specified for a set operation is zero or less.
    /// </exception>
    public TimeSpan WaitTime {
      get {
        return _services.WaitTime;
      }

      set {
        _services.WaitTime = value;
      }
    }

    /// <summary>
    /// Gets the management function for the WebSocket services provided by
    /// the server.
    /// </summary>
    /// <value>
    /// A <see cref="WebSocketServiceManager"/> that manages the WebSocket
    /// services provided by the server.
    /// </value>
    public WebSocketServiceManager WebSocketServices {
      get {
        return _services;
      }
    }

    #endregion

    #region Public Events

    /// <summary>
    /// Occurs when the server receives an HTTP CONNECT request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnConnect;

    /// <summary>
    /// Occurs when the server receives an HTTP DELETE request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnDelete;

    /// <summary>
    /// Occurs when the server receives an HTTP GET request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnGet;

    /// <summary>
    /// Occurs when the server receives an HTTP HEAD request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnHead;

    /// <summary>
    /// Occurs when the server receives an HTTP OPTIONS request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnOptions;

    /// <summary>
    /// Occurs when the server receives an HTTP POST request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnPost;

    /// <summary>
    /// Occurs when the server receives an HTTP PUT request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnPut;

    /// <summary>
    /// Occurs when the server receives an HTTP TRACE request.
    /// </summary>
    public event EventHandler<HttpRequestEventArgs> OnTrace;

    #endregion

    #region Private Methods

    private void abort ()
    {
      lock (_sync) {
        if (_state != ServerState.Start)
          return;

        _state = ServerState.ShuttingDown;
      }

      try {
        _services.Stop (1006, String.Empty);
      }
      catch (Exception ex) {
        _log.Fatal (ex.Message);
        _log.Debug (ex.ToString ());
      }

      try {
        _listener.Abort ();
      }
      catch (Exception ex) {
        _log.Fatal (ex.Message);
        _log.Debug (ex.ToString ());
      }

      _state = ServerState.Stop;
    }

    private bool canSet ()
    {
      return _state == ServerState.Ready || _state == ServerState.Stop;
    }

    private bool checkCertificate (out string message)
    {
      message = null;

      var byUser = _listener.SslConfiguration.ServerCertificate != null;

      var path = _listener.CertificateFolderPath;
      var withPort = EndPointListener.CertificateExists (_port, path);

      var either = byUser || withPort;

      if (!either) {
        message = "There is no server certificate for secure connection.";

        return false;
      }

      var both = byUser && withPort;

      if (both) {
        var msg = "The server certificate associated with the port is used.";

        _log.Warn (msg);
      }

      return true;
    }

    private static HttpListener createListener (
      string hostname,
      int port,
      bool secure
    )
    {
      var ret = new HttpListener ();

      var fmt = "{0}://{1}:{2}/";
      var schm = secure ? "https" : "http";
      var pref = String.Format (fmt, schm, hostname, port);

      ret.Prefixes.Add (pref);

      return ret;
    }

    private void init (
      string hostname,
      System.Net.IPAddress address,
      int port,
      bool secure
    )
    {
      _address = address;
      _port = port;
      _isSecure = secure;

      _docRootPath = "./Public";
      _listener = createListener (hostname, port, secure);
      _log = _listener.Log;
      _services = new WebSocketServiceManager (_log);
      _sync = new object ();
    }

    private void processRequest (HttpListenerContext context)
    {
      var method = context.Request.HttpMethod;
      var evt = method == "GET"
                ? OnGet
                : method == "HEAD"
                  ? OnHead
                  : method == "POST"
                    ? OnPost
                    : method == "PUT"
                      ? OnPut
                      : method == "DELETE"
                        ? OnDelete
                        : method == "CONNECT"
                          ? OnConnect
                          : method == "OPTIONS"
                            ? OnOptions
                            : method == "TRACE"
                              ? OnTrace
                              : null;

      if (evt == null) {
        context.ErrorStatusCode = 501;

        context.SendError ();

        return;
      }

      var e = new HttpRequestEventArgs (context, _docRootPath);

      evt (this, e);

      context.Response.Close ();
    }

    private void processRequest (HttpListenerWebSocketContext context)
    {
      var uri = context.RequestUri;

      if (uri == null) {
        context.Close (HttpStatusCode.BadRequest);

        return;
      }

      var path = uri.AbsolutePath;

      if (path.IndexOfAny (new[] { '%', '+' }) > -1)
        path = HttpUtility.UrlDecode (path, Encoding.UTF8);

      WebSocketServiceHost host;

      if (!_services.InternalTryGetServiceHost (path, out host)) {
        context.Close (HttpStatusCode.NotImplemented);

        return;
      }

      host.StartSession (context);
    }

    private void receiveRequest ()
    {
      while (true) {
        HttpListenerContext ctx = null;

        try {
          ctx = _listener.GetContext ();

          ThreadPool.QueueUserWorkItem (
            state => {
              try {
                if (ctx.Request.IsUpgradeRequest ("websocket")) {
                  processRequest (ctx.GetWebSocketContext (null));

                  return;
                }

                processRequest (ctx);
              }
              catch (Exception ex) {
                _log.Error (ex.Message);
                _log.Debug (ex.ToString ());

                ctx.Connection.Close (true);
              }
            }
          );
        }
        catch (HttpListenerException ex) {
          if (_state == ServerState.ShuttingDown)
            return;

          _log.Fatal (ex.Message);
          _log.Debug (ex.ToString ());

          break;
        }
        catch (InvalidOperationException ex) {
          if (_state == ServerState.ShuttingDown)
            return;

          _log.Fatal (ex.Message);
          _log.Debug (ex.ToString ());

          break;
        }
        catch (Exception ex) {
          _log.Fatal (ex.Message);
          _log.Debug (ex.ToString ());

          if (ctx != null)
            ctx.Connection.Close (true);

          if (_state == ServerState.ShuttingDown)
            return;

          break;
        }
      }

      abort ();
    }

    private void start ()
    {
      lock (_sync) {
        if (_state == ServerState.Start || _state == ServerState.ShuttingDown)
          return;

        if (_isSecure) {
          string msg;

          if (!checkCertificate (out msg))
            throw new InvalidOperationException (msg);
        }

        _services.Start ();

        try {
          startReceiving ();
        }
        catch {
          _services.Stop (1011, String.Empty);

          throw;
        }

        _state = ServerState.Start;
      }
    }

    private void startReceiving ()
    {
      try {
        _listener.Start ();
      }
      catch (Exception ex) {
        var msg = "The underlying listener has failed to start.";

        throw new InvalidOperationException (msg, ex);
      }

      var receiver = new ThreadStart (receiveRequest);
      _receiveThread = new Thread (receiver);
      _receiveThread.IsBackground = true;

      _receiveThread.Start ();
    }

    private void stop (ushort code, string reason)
    {
      lock (_sync) {
        if (_state != ServerState.Start)
          return;

        _state = ServerState.ShuttingDown;
      }

      try {
        _services.Stop (code, reason);
      }
      catch (Exception ex) {
        _log.Fatal (ex.Message);
        _log.Debug (ex.ToString ());
      }

      try {
        var timeout = 5000;

        stopReceiving (timeout);
      }
      catch (Exception ex) {
        _log.Fatal (ex.Message);
        _log.Debug (ex.ToString ());
      }

      _state = ServerState.Stop;
    }

    private void stopReceiving (int millisecondsTimeout)
    {
      _listener.Stop ();
      _receiveThread.Join (millisecondsTimeout);
    }

    private static bool tryCreateUri (
      string uriString,
      out Uri result,
      out string message
    )
    {
      result = null;
      message = null;

      var uri = uriString.ToUri ();

      if (uri == null) {
        message = "An invalid URI string.";

        return false;
      }

      if (!uri.IsAbsoluteUri) {
        message = "A relative URI.";

        return false;
      }

      var schm = uri.Scheme;
      var isHttpSchm = schm == "http" || schm == "https";

      if (!isHttpSchm) {
        message = "The scheme part is not 'http' or 'https'.";

        return false;
      }

      if (uri.PathAndQuery != "/") {
        message = "It includes either or both path and query components.";

        return false;
      }

      if (uri.Fragment.Length > 0) {
        message = "It includes the fragment component.";

        return false;
      }

      if (uri.Port == 0) {
        message = "The port part is zero.";

        return false;
      }

      result = uri;

      return true;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Adds a WebSocket service with the specified behavior and path.
    /// </summary>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to add.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <typeparam name="TBehavior">
    ///   <para>
    ///   The type of the behavior for the service.
    ///   </para>
    ///   <para>
    ///   It must inherit the <see cref="WebSocketBehavior"/> class.
    ///   </para>
    ///   <para>
    ///   Also it must have a public parameterless constructor.
    ///   </para>
    /// </typeparam>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is already in use.
    ///   </para>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    public void AddWebSocketService<TBehavior> (string path)
      where TBehavior : WebSocketBehavior, new ()
    {
      _services.AddService<TBehavior> (path, null);
    }

    /// <summary>
    /// Adds a WebSocket service with the specified behavior, path,
    /// and initializer.
    /// </summary>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to add.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <param name="initializer">
    ///   <para>
    ///   An <see cref="T:System.Action{TBehavior}"/> delegate.
    ///   </para>
    ///   <para>
    ///   It specifies the delegate called when the service initializes
    ///   a new session instance.
    ///   </para>
    ///   <para>
    ///   <see langword="null"/> if not necessary.
    ///   </para>
    /// </param>
    /// <typeparam name="TBehavior">
    ///   <para>
    ///   The type of the behavior for the service.
    ///   </para>
    ///   <para>
    ///   It must inherit the <see cref="WebSocketBehavior"/> class.
    ///   </para>
    ///   <para>
    ///   Also it must have a public parameterless constructor.
    ///   </para>
    /// </typeparam>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is already in use.
    ///   </para>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    public void AddWebSocketService<TBehavior> (
      string path,
      Action<TBehavior> initializer
    )
      where TBehavior : WebSocketBehavior, new ()
    {
      _services.AddService<TBehavior> (path, initializer);
    }

    /// <summary>
    /// Removes a WebSocket service with the specified path.
    /// </summary>
    /// <remarks>
    /// The service is stopped with close status 1001 (going away)
    /// if the current state of the service is Start.
    /// </remarks>
    /// <returns>
    /// <c>true</c> if the service is successfully found and removed;
    /// otherwise, <c>false</c>.
    /// </returns>
    /// <param name="path">
    ///   <para>
    ///   A <see cref="string"/> that specifies an absolute path to
    ///   the service to remove.
    ///   </para>
    ///   <para>
    ///   / is trimmed from the end of the string if present.
    ///   </para>
    /// </param>
    /// <exception cref="ArgumentException">
    ///   <para>
    ///   <paramref name="path"/> is an empty string.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> is not an absolute path.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   <paramref name="path"/> includes either or both
    ///   query and fragment components.
    ///   </para>
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="path"/> is <see langword="null"/>.
    /// </exception>
    public bool RemoveWebSocketService (string path)
    {
      return _services.RemoveService (path);
    }

    /// <summary>
    /// Starts receiving incoming requests.
    /// </summary>
    /// <remarks>
    /// This method works if the current state of the server is Ready or Stop.
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    ///   <para>
    ///   There is no server certificate for secure connection.
    ///   </para>
    ///   <para>
    ///   -or-
    ///   </para>
    ///   <para>
    ///   The underlying <see cref="HttpListener"/> has failed to start.
    ///   </para>
    /// </exception>
    public void Start ()
    {
      if (_state == ServerState.Start || _state == ServerState.ShuttingDown)
        return;

      start ();
    }

    /// <summary>
    /// Stops receiving incoming requests.
    /// </summary>
    /// <remarks>
    /// This method works if the current state of the server is Start.
    /// </remarks>
    public void Stop ()
    {
      if (_state != ServerState.Start)
        return;

      stop (1001, String.Empty);
    }

    #endregion
  }
}
