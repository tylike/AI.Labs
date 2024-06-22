using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Color = Microsoft.Xna.Framework.Color;
using Keys = Microsoft.Xna.Framework.Input.Keys;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private SpriteBatch _spriteBatch;

    // Player and animation
    private Texture2D _characterTexture;
    private int _characterWidth = 144;
    private int _characterHeight = 128;
    private Vector2 _playerPosition = new Vector2(200, 200);
    private float _playerSpeed = 100f;
    private int _currentFrame = 0;
    private double _animationTimer = 0;
    private int _direction = 0; // 0: Down, 1: Left, 2: Right, 3: Up

    // Map
    private Texture2D _tilesetTexture;
    private int[,] _mapData;
    private int _tileWidth = 48;
    private int _tileHeight = 48;
    private int _mapWidth;
    private int _mapHeight;

    // Weather
    private List<Vector2> _rainPositions;
    private int _numRainDrops = 100;
    private Texture2D _rainTexture;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        // Initialize the rain positions
        _rainPositions = new List<Vector2>();
        for (int i = 0; i < _numRainDrops; i++)
        {
            _rainPositions.Add(new Vector2(
                Random.Shared.Next(0, _graphics.PreferredBackBufferWidth),
                Random.Shared.Next(0, _graphics.PreferredBackBufferHeight)
            ));
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);

        // Load the character texture
        _characterTexture = Texture2D.FromFile(GraphicsDevice, "D:\\chatgpt.rmmz\\img\\characters\\Actor2.png");

        // Load the tileset texture
        _tilesetTexture = Texture2D.FromFile(GraphicsDevice, "D:\\chatgpt.rmmz\\img\\tilesets\\fsm_Town01_A2.png");

        // Load the map
        string mapJson = File.ReadAllText("D:\\chatgpt.rmmz\\data\\Map005.json");
        dynamic map = JsonConvert.DeserializeObject<dynamic>(mapJson);
        _mapWidth = map.width;
        _mapHeight = map.height;
        _mapData = new int[_mapWidth, _mapHeight];
        int index = 0;
        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                _mapData[x, y] = map.data[index];
                index++;
            }
        }

        // Create a simple white texture for the rain
        _rainTexture = new Texture2D(GraphicsDevice, 1, 1);
        _rainTexture.SetData(new[] { Color.White });
    }

    protected override void Update(GameTime gameTime)
    {
        var keyboardState = Keyboard.GetState();
        Vector2 moveDirection = Vector2.Zero;

        if (keyboardState.IsKeyDown(Keys.W))
        {
            moveDirection.Y = -1;
            _direction = 3;
        }
        if (keyboardState.IsKeyDown(Keys.S))
        {
            moveDirection.Y = 1;
            _direction = 0;
        }
        if (keyboardState.IsKeyDown(Keys.A))
        {
            moveDirection.X = -1;
            _direction = 1;
        }
        if (keyboardState.IsKeyDown(Keys.D))
        {
            moveDirection.X = 1;
            _direction = 2;
        }

        if (moveDirection != Vector2.Zero)
        {
            moveDirection.Normalize();
            _playerPosition += moveDirection * _playerSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update animation
            _animationTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_animationTimer > 200)
            {
                _animationTimer = 0;
                _currentFrame = (_currentFrame + 1) % 3;
            }
        }

        // Update rain
        for (int i = 0; i < _rainPositions.Count; i++)
        {
            _rainPositions[i] = new Vector2(
                _rainPositions[i].X + (float)(gameTime.ElapsedGameTime.TotalSeconds * 50),
                _rainPositions[i].Y + (float)(gameTime.ElapsedGameTime.TotalSeconds * 200)
            );

            if (_rainPositions[i].Y > _graphics.PreferredBackBufferHeight)
            {
                _rainPositions[i] = new Vector2(
                    Random.Shared.Next(0, _graphics.PreferredBackBufferWidth),
                    -10
                );
            }
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.CornflowerBlue);

        _spriteBatch.Begin();

        // Draw the map
        for (int y = 0; y < _mapHeight; y++)
        {
            for (int x = 0; x < _mapWidth; x++)
            {
                int tileIndex = _mapData[x, y] - 1;
                if (tileIndex >= 0)
                {
                    int tilesPerRow = _tilesetTexture.Width / _tileWidth;
                    int tileX = tileIndex % tilesPerRow;
                    int tileY = tileIndex / tilesPerRow;
                    Rectangle sourceRectangle = new Rectangle(tileX * _tileWidth, tileY * _tileHeight, _tileWidth, _tileHeight);
                    Rectangle destinationRectangle = new Rectangle(x * _tileWidth, y * _tileHeight, _tileWidth, _tileHeight);
                    _spriteBatch.Draw(_tilesetTexture, destinationRectangle, sourceRectangle, Color.White);
                }
            }
        }

        // Draw the player
        Rectangle sourceRect = new Rectangle(
            _currentFrame * _characterWidth,
            _direction * _characterHeight,
            _characterWidth,
            _characterHeight
        );
        _spriteBatch.Draw(_characterTexture, _playerPosition, sourceRect, Color.White, 0f, new Vector2(_characterWidth / 2, _characterHeight / 2), 1.0f, SpriteEffects.None, 0f);

        // Draw rain
        foreach (var rainPos in _rainPositions)
        {
            _spriteBatch.Draw(_rainTexture, new Rectangle((int)rainPos.X, (int)rainPos.Y, 2, 10), Color.Blue * 0.7f);
        }

        _spriteBatch.End();

        base.Draw(gameTime);
    }
}

