using System;
using System.Threading.Tasks;

namespace FoTos.Services.GoogleUploader
{
    public interface IGPhotosUploader
    {
        Task Upload(String filename);
    }
}
