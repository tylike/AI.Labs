﻿namespace OpenAI.ObjectModels;

public static class UploadFilePurposes
{
    public enum UploadFilePurpose
    {
        FineTune,
        FineTuneResults
    }

    public const string FineTune = "fine-tune";
    public const string FineTuneResults = "fine-tune-results";

    public static string EnumToString(this UploadFilePurpose uploadFilePurpose)
    {
        return uploadFilePurpose switch
        {
            UploadFilePurpose.FineTune => FineTune,
            UploadFilePurpose.FineTuneResults => FineTuneResults,
            _ => throw new ArgumentOutOfRangeException(nameof(uploadFilePurpose), uploadFilePurpose, null)
        };
    }

    public static UploadFilePurpose ToEnum(string filePurpose)
    {
        return filePurpose switch
        {
            FineTune => UploadFilePurpose.FineTune,
            FineTuneResults => UploadFilePurpose.FineTuneResults,
            _ => throw new ArgumentOutOfRangeException(nameof(filePurpose), filePurpose, null)
        };
    }
}