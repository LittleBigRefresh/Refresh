// Global using directives

global using static System.Net.HttpStatusCode;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;

global using Refresh.GameServer.Extensions;

#if !POSTGRES
global using Realms;
global using Bunkum.RealmDatabase;
#endif

#if POSTGRES
global using Bunkum.Core.Database;
global using Microsoft.EntityFrameworkCore;
#endif