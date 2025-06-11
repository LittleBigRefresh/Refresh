global using static System.Net.HttpStatusCode;
global using Newtonsoft.Json;
global using Newtonsoft.Json.Serialization;

global using Refresh.Database.Extensions;
global using Refresh.Common.Extensions;

global using Refresh.Database.Compatibility;

#if POSTGRES
global using Bunkum.Core.Database;
global using Microsoft.EntityFrameworkCore;
global using System.ComponentModel.DataAnnotations;
#else
global using Realms;
global using Bunkum.RealmDatabase;
#endif