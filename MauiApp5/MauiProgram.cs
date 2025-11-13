using Microsoft.Extensions.Logging;
using MauiApp5.Services;

namespace MauiApp5
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()              
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
    		builder.Logging.AddDebug();
#endif
            // 1. REGISTRO CONDICIONAL DO SERVIÇO DE IMPRESSÃO
            #if ANDROID
                builder.Services.AddTransient<IImpressoraServiceCupom, MauiApp5.Platforms.Android.ImpressoraServiceCupom>();
            #endif


            return builder.Build();
        }
    }
}
