using Core.Extensions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Core.AppContexts
{
    public static class AppContexts
    {
        private static IHttpContextAccessor httpContextAccessor;
        private static List<Connection> connectionList { get; set; }

        public static void Configure(IHttpContextAccessor _httpContextAccessor)
        {
            httpContextAccessor = _httpContextAccessor;
        }
        public static void ConfigureConnectionStrings(Dictionary<string, string> connections)
        {
            var server = string.Empty;
            var database = string.Empty;
            var userId = string.Empty;
            var password = string.Empty;

            connectionList = new List<Connection>();

            foreach (var conString in connections)
            {
                var connectionString = conString.Value;
                var serverInfo = connectionString.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (var info in serverInfo)
                {
                    if (info.Trim().ToLower().StartsWith("data source") || info.Trim().ToLower().StartsWith("server"))
                    {
                        server = info.Split('=')[1].Trim();
                    }
                    else if (info.Trim().ToLower().StartsWith("user id") || info.Trim().ToLower().StartsWith("user") || info.Trim().ToLower().StartsWith("uid"))
                    {
                        userId = info.Split('=')[1].Trim();
                    }
                    else if (info.Trim().ToLower().StartsWith("pwd") || info.Trim().ToLower().StartsWith("password"))
                    {
                        password = info.Split('=')[1].Trim();
                    }
                    else if (info.Trim().ToLower().StartsWith("initial catalog") || info.Trim().ToLower().StartsWith("database"))
                    {
                        database = info.Split('=')[1].Trim();
                    }
                }

                var connection = new Connection
                {
                    Name = conString.Key,
                    ConnectionString = connectionString,
                    Server = server,
                    Database = database,
                    UserId = userId,
                    Password = password
                };

                connectionList.Add(connection);
            }
        }
        public static string GetDatabaseName(string conName)
        {
            var connection = connectionList.Find(con => con.Name.Equals(conName));
            return connection.IsNull() ? string.Empty : connection.Database;
        }
        public static string GetConnectionString(string conName)
        {
            var connection = connectionList.Find(con => con.Name.Equals(conName));
            return connection.IsNull() ? string.Empty : connection.ConnectionString;
        }

        public static HttpContext Current => httpContextAccessor.HttpContext;
        public static UserPrincipal User
        {
            get
            {
                var user = new UserPrincipal { UserID = -1, UserName = "System", CompanyID = "nagad" };

                if (Current.Items["ActiveUser"].IsNotNull())
                    user = Current.Items["ActiveUser"] as UserPrincipal;

                return user;
            }
        }

        public static byte[] MenuApipathsViewlist
        {
            get
            {
                var session = Current.Session;
                byte[] abc;
                session.TryGetValue("key", out abc);
                return abc;
            }
        }

        public static void Resolve<T>()
        {
            Current.RequestServices.GetService(typeof(T));
        }

        public static void Resolve(Type type)
        {
            Current.RequestServices.GetService(type);
        }

        public static T GetInstance<T>()
        {
            return (T)Current.RequestServices.GetService(typeof(T));
        }

        public static object GetInstance(Type type)
        {
            return Current.RequestServices.GetService(type);
        }

        public static void SetActiveDbContext<T>(T context)
        {
            if (Current.IsNotNull())
            {
                var list = new List<T>();

                if (Current.Items["ActiveDbContext"].IsNotNull())
                    list = Current.Items["ActiveDbContext"] as List<T>;

                if (list != null)
                {
                    list.Add(context);
                    Current.Items["ActiveDbContext"] = list;
                }
            }
        }

        public static List<T> GetActiveDbContexts<T>()
        {
            var list = new List<T>();

            if (Current.Items["ActiveDbContext"].IsNotNull())
                list = Current.Items["ActiveDbContext"] as List<T>;

            return list;
        }

        public static void SetUserInfo(UserPrincipal user)
        {
            Current.Items["ActiveUser"] = user;
        }

        //public static void SetMenuApipathsViewlist(string list)
        //{
        //    Current.Session.Set("key", Encoding.ASCII.GetBytes(list));
        //}

        public static bool IsValidUser(string ipAddress)
        {
            return ipAddress == GetIPAddress();
        }

        public static void SetDefaultUser()
        {
            var newUser = new UserPrincipal
            {
                LogedID = -1,
                UserID = -1,
                UserName = "System",
                IsAdmin = true,
                ApplicationID = -1,
                CompanyID = "-1",
                CompanyName = "System",
                LogInDateTime = DateTime.Now,
                IPAddress = GetIPAddress(),
                FullName = ""
            };
        }

        public static string GetIPAddress()
        {
            var ips = Current?.Connection?.RemoteIpAddress?.ToString();

            if (ips == null) return null;

            ips = ips.IsNotNullOrEmpty() ? ips.Split(',')[0] : Current?.Connection?.RemoteIpAddress?.MapToIPv4().ToString();

            if (ips.IsNullOrEmpty() || ips == "::1")
            {
                ips = GetLocalIPAddress();
            }

            return ips;
        }

        public static bool IsRemoteIP(List<string> ips)
        {
            return !ips.Contains(GetIPAddress());
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return "192.168.0.1";
        }

        public static string GetMachineName(string clientIP)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(clientIP);
                if (hostEntry.IsNull()) return "default";
                return hostEntry.HostName.IsNullOrEmpty() ? "default" : hostEntry.HostName;
            }
            catch
            {
                return "default";
            }
        }
    }
}
