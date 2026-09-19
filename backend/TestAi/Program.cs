using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using StudyPlatform.Infrastructure.AiProviders;

namespace TestApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            try {
                var config = new ConfigurationBuilder()
                    .SetBasePath(@"c:\Projetos\tcc-plataforma-estudos\backend\StudyPlatform.API")
                    .AddJsonFile("appsettings.Development.json")
                    .Build();

                Console.WriteLine("Testando Groq...");
                var groq = new GroqProvider(config);
                var groqRes = await groq.GenerateAsync("O que é fotossíntese?", "A fotossíntese é o processo das plantas.");
                Console.WriteLine("Groq gerou: " + groqRes);

                Console.WriteLine("Testando Gemini Embeddings...");
                var gemini = new GeminiProvider(config);
                var emb = await gemini.EmbedAsync("Teste de vetor");
                Console.WriteLine($"Gemini gerou vetor de tamanho: {emb.Length}");
            } catch (Exception ex) {
                Console.WriteLine("ERRO FATAL: " + ex.ToString());
            }
        }
    }
}