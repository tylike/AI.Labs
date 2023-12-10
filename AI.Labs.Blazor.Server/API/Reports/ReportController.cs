#nullable enable
using DevExpress.ExpressApp.ReportsV2.Services;
using DevExpress.ExpressApp.ReportsV2;
using DevExpress.Persistent.BaseImpl;
using DevExpress.Xpo;
using DevExpress.Xpo.DB;
using DevExpress.XtraPrinting;
using DevExpress.XtraReports.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AI.Labs.WebApi.Reports;

[Authorize]
[Route("api/[controller]")]
// This is a WebApi Reports controller sample.
public class ReportController : ControllerBase {
    private readonly IReportExportService service;

    public ReportController(IReportExportService reportExportService) {
        service = reportExportService;
    }

    private void ApplyParametersFromQuery(XtraReport report) {
        foreach(var parameter in report.Parameters) {
            var queryParam = Request.Query[parameter.Description];
            if(queryParam.Count > 0) {
                parameter.Value = queryParam.First();
            }
        }
    }
    private SortProperty[]? LoadSortPropertiesFromQuery() {
        if(Request.Query.Keys.Contains("sortProperty")) {
            var queryParam = Request.Query["sortProperty"];
            SortProperty[] result = new SortProperty[queryParam.Count];
            for(int i = 0; i < queryParam.Count; i++) {
                string[] paramData = queryParam[i].Split(",");
                result[i] = new SortProperty(paramData[0], (SortingDirection)Enum.Parse(typeof(SortingDirection), paramData[1]));
            }
            return result;
        }
        return null;
    }

    private async Task<object> GetReportContentAsync(XtraReport report, ExportTarget fileType) {
        Stream ms = await service.ExportReportAsync(report, fileType);
        HttpContext.Response.RegisterForDispose(ms);
        return File(ms, service.GetContentType(fileType), $"{report.DisplayName}.{service.GetFileExtension(fileType)}");
    }

    [HttpGet("DownloadByKey({key})")]
    [SwaggerOperation("Gets the contents of a report specified by its key in the specified file format filtered based on the specified condition.", "For more information, refer to the following article: <a href='https://docs.devexpress.com/eXpressAppFramework/404176/backend-web-api-service/obtain-a-report-from-a-web-api-controller-endpoint'>Obtain a Report from a Web API Controller Endpoint</a>.")]
    public async Task<object> DownloadByKey(
        [SwaggerParameter("A primary key value that uniquely identifies a report. <br/>Example: '83978d7f-82b7-4380-979a-08db4587a66b'")]
        string key,
        [FromQuery, SwaggerParameter("The file type in which to download the report.")]
        ExportTarget fileType = ExportTarget.Pdf,
        [FromQuery, SwaggerParameter("A condition used to filter the report's data. <br/>Example: \"[FirstName] = 'Aaron'\"")]
        string? criteria = null) {
        using var report = service.LoadReport<ReportDataV2>(key);
        ApplyParametersFromQuery(report);
        SortProperty[]? sortProperties = LoadSortPropertiesFromQuery();
        service.SetupReport(report, criteria, sortProperties);
        return await GetReportContentAsync(report, fileType);
    }

    [HttpGet("DownloadByName({displayName})")]
    [SwaggerOperation("Gets the contents of a report specified by its display name in the specified file format filtered based on the specified condition.", "For more information, refer to the following article: <a href='https://docs.devexpress.com/eXpressAppFramework/404176/backend-web-api-service/obtain-a-report-from-a-web-api-controller-endpoint'>Obtain a Report from a Web API Controller Endpoint</a>.")]
    public async Task<object> DownloadByName(
        [SwaggerParameter("The display name of a report to download. <br/>Example: 'Employee List Report'")]
        string displayName,
        [FromQuery, SwaggerParameter("The file type in which to download the report.")]
        ExportTarget fileType = ExportTarget.Pdf,
        [FromQuery, SwaggerParameter("A condition used to filter the report's data. <br/>Example: \"[FirstName] = 'Aaron'\"")]
        string? criteria = null) {
        if(!string.IsNullOrEmpty(displayName)) {
            using var report = service.LoadReport<ReportDataV2>(data => data.DisplayName == displayName);
            ApplyParametersFromQuery(report);
            SortProperty[]? sortProperties = LoadSortPropertiesFromQuery();
            service.SetupReport(report, criteria, sortProperties);
            return await GetReportContentAsync(report, fileType);
        }
        return NotFound();
    }
}
#nullable restore