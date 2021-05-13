using FaceId.Infrastructure.Exceptions;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FaceId.Infrastructure
{
    public class FaceService : IFaceService
    {
        const string SubscriptionKey = "ed283cc7beaf4f80aae468793dfcaf93";
        const string Endpoint = "https://digitalgrading-cognitiveface.cognitiveservices.azure.com/";
        const string Model = RecognitionModel.Recognition03;
        const string PersonGroupId = "users";

        private readonly IFaceClient _client;

        public FaceService()
        {
            _client = new FaceClient(new ApiKeyServiceClientCredentials(SubscriptionKey)) { Endpoint = Endpoint };

        }

        /// <summary>
        /// Identifica personas en un grupo
        /// </summary>
        /// <param name="url">La URL de la imagen a identificar</param>
        /// <returns></returns>
        public async Task TrainPersonGroupAsync(string registerUserPhotoUrl, string email)
        {
            try
            {
                await _client.PersonGroup.GetAsync(PersonGroupId);
            }
            catch (Exception ex)
            {
                await _client.PersonGroup.CreateAsync(PersonGroupId, PersonGroupId, recognitionModel: Model);   
            }
            // Los ID son los nombres, las URL son fotos con su cara
            var peopleDictionary = new Dictionary<string, string[]>();
            peopleDictionary.Add(email, new[] { registerUserPhotoUrl });

            // Agrupa personas en un mismo grupo
            foreach (var groupedFace in peopleDictionary.Keys)
            {
                await Task.Delay(250);

                var person = await _client.PersonGroupPerson.CreateAsync(PersonGroupId, groupedFace);

                foreach (var similarImage in peopleDictionary[groupedFace])
                {
                    await _client.PersonGroupPerson.AddFaceFromUrlAsync(PersonGroupId, person.PersonId, $"{similarImage}", similarImage);
                }
            }

            // Entrena el modelo
            await _client.PersonGroup.TrainAsync(PersonGroupId);

            while (true)
            {
                await Task.Delay(1000);

                var trainingStatus = await _client.PersonGroup.GetTrainingStatusAsync(PersonGroupId);

                if (trainingStatus.Status == TrainingStatusType.Succeeded)
                {
                    break;
                }
            }
        }

        public async Task<bool> ValidatePerson(string personFaceImageUrl)
        {
            // Identifica las caras en la imagen con las que tiene entrenado el modelo
            try
            {
                var sourceFaceIds = new List<Guid>();
                var detectedFaces = await DetectFaceAsync(personFaceImageUrl);

                foreach (var detectedFace in detectedFaces)
                {
                    sourceFaceIds.Add(detectedFace.FaceId.Value);
                }
                var identifyResults = await _client.Face.IdentifyAsync(sourceFaceIds, PersonGroupId);
                foreach (var identifyResult in identifyResults)
                {
                    if (identifyResult.Candidates.Any())
                    {
                        await _client.PersonGroupPerson.GetAsync(PersonGroupId, identifyResult.Candidates[0].PersonId);
                        if (identifyResult.Candidates[0].Confidence > 0.5)
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        private async Task<List<DetectedFace>> DetectFaceAsync(string url)
        {
            var detectedFaces = await _client.Face.DetectWithUrlAsync(url, recognitionModel: Model, detectionModel: DetectionModel.Detection02);
            return detectedFaces.ToList();
        }
    }
}
