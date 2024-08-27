using System;
using Vectorization.Operations;
using NLog;

namespace Vectorization
{
    public static class VectorizationSystemStartup
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public static void Initialize()
        {
            Logger.Info("Initializing Vectorization System");

            try
            {
                OperationRegistry.DiscoverHandlers();
                Logger.Info("Operation handlers discovered and registered successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error during operation handler discovery");
                throw;
            }

            // Add any other initialization steps here

            Logger.Info("Vectorization System initialized successfully");
        }
    }
}