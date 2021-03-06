﻿namespace Oxide.Ext.Discord
{
    using System.Collections.Generic;
    using System.Linq;
    using Oxide.Core.Plugins;
    using Oxide.Ext.Discord.Exceptions;

    public class Discord
    {
        public static List<DiscordClient> Clients { get; private set; } = new List<DiscordClient>();

        public static void CreateClient(Plugin plugin, string apiKey)
        {
            if (plugin == null)
            {
                throw new PluginNullException();
            }

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new APIKeyException();
            }

            var settings = new DiscordSettings()
            {
                ApiToken = apiKey
            };

            CreateClient(plugin, settings);
        }

        public static void CreateClient(Plugin plugin, DiscordSettings settings)
        {
            if (plugin == null)
            {
                throw new PluginNullException();
            }
            
            if (settings == null)
            {
                throw new SettingsNullException();
            }

            if (string.IsNullOrEmpty(settings.ApiToken))
            {
                throw new APIKeyException();
            }

            // Find an existing DiscordClient and update it 
            var client = Clients.FirstOrDefault(x => x.Plugins.Any(p => p.Title == plugin.Title));
            if (client != null)
            {
                if (client.Settings.ApiToken != settings.ApiToken)
                {
                    throw new LimitedClientException();
                }

                var existingPlugins = client.Plugins.Where(x => x.Title == plugin.Title).ToList();
                existingPlugins.ForEach(x => client.Plugins.Remove(x));

                client.RegisterPlugin(plugin);
                client.UpdatePluginReference(plugin);
                client.Settings = settings;
                client.CallHook("DiscordSocket_Initialized", plugin);
                return;
            }

            // Create a new DiscordClient
            var newClient = new DiscordClient();
            Clients.Add(newClient);
            newClient.Initialize(plugin, settings);
        }

        public static void CloseClient(DiscordClient client)
        {
            if (client == null) return;

            client.Disconnect();
            Clients.Remove(client);
        }
    }
}
