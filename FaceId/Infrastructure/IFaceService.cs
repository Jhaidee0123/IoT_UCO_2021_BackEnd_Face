using System.Threading.Tasks;

namespace FaceId.Infrastructure
{
    public interface IFaceService
    {
        Task TrainPersonGroupAsync(string registerUserPhotoUrl, string email);
        Task<bool> ValidatePerson(string personFaceImageUrl);
    }
}