using System;
using LinnworksMacroHelpers;
using Microsoft.Extensions.Configuration;

namespace WaspIntegration.Service.Interfaces
{
    public interface IManifestService
    {
        void UploadManifest(Guid? locationId,IConfiguration configuration, string token);
        LinnworksMacroBase LinnWorks { get; set; }
    }
}
