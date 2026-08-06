using OpenAI;
using OpenAI.Chat;
using System.ClientModel;

namespace IAX.IXApi.Bootstrap.Extensions
{
    public static class OpenAiExtensions
    {
        public static IServiceCollection RegisterOpenAI(this IServiceCollection services, string model, ApiKeyCredential apiKeyCredential)
        {
            OpenAIClient openAIClient = new OpenAIClient(apiKeyCredential);

            ChatClient chatClient = new ChatClient(model, apiKeyCredential);

            services.AddSingleton(chatClient);

            return services;
        }
    }
}
