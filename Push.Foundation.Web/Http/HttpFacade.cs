﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Push.Foundation.Web.Misc;

namespace Push.Foundation.Web.Http
{
    public class HttpFacade : IDisposable
    {
        private readonly HttpClient _client;
        private readonly DurableExecContext _context;

        public HttpFacade(HttpClient client, DurableExecContext context)
        {
            _client = client;
            _context = context;
        }

        public virtual ResponseEnvelope Get(
                string url,
                Dictionary<string, string> headers = null,
                string contentType = "application/json; charset=utf-8")
        {
            _context.Logger.Debug($"HTTP GET on {url}");

            var response =
                DurableExec.Do(
                    () => _client.GetAsync(url).Result, _context);

            return response
                    .ToEnvelope()
                    .ProcessStatusCodes();
        }

        public virtual ResponseEnvelope Post(string url, string content)
        {
            _context.Logger.Debug($"HTTP POST on {url}");

            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");
            
            var response =
                DurableExec.Do(
                    () => _client.PostAsync(url, httpContent).Result, _context);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }

        public virtual ResponseEnvelope Put(string url, string content)
        {
            _context.Logger.Debug($"HTTP PUT on {url}");

            var httpContent
                = new StringContent(content, Encoding.UTF8, "application/json");

            var response =
                DurableExec.Do(
                    () => _client.PutAsync(url, httpContent).Result, _context);

            return response
                .ToEnvelope()
                .ProcessStatusCodes();
        }

        public void Dispose()
        {
            _client?.Dispose();
        }
    }
}
