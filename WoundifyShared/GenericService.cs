using System;
using System.Collections.Generic;
using System.Linq; // for SelectMany
using System.Reflection; // for GetTypeInfo()
using System.Threading.Tasks;

namespace WoundifyShared
{
    public interface IGenericService : IService
    {
    }
    public interface IGenericServiceResponse : IServiceResponse
    {
    }
    public interface IGenericCallServices : ICallServices<IGenericServiceResponse>
    {
    }
    public interface IGenericRunServices : IRunServices<IGenericService, IGenericServiceResponse>
    {
    }
    public interface IGenericServiceCallRun : IGenericCallServices, IGenericRunServices // templateless
    {
    }

    public class GenericService : IGenericService
    {
    }
    public class GenericServiceResponse : ServiceResponse
    {
    }
    public class GenericCallServices : CallServices<IGenericService, IGenericServiceResponse>, IGenericCallServices
    {
        public GenericCallServices(Settings.Service service) : base(service)
        {
        }
    }
    public class GenericRunServices : RunServices<IGenericService, IGenericServiceResponse>
    {
    }

    // Templated class to call services
    public class CallServiceResponse<TServiceResponse> : ServiceResponse
    {
        public CallServiceResponse(Settings.Service service)
        {
            this.Service = service;
        }
    }

    public interface ICallServices<TServiceResponse>
    {
        System.Collections.Generic.Dictionary<string, int> CompatibileArgTypesPerProvider { get; set; }
        System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>> CallServiceAsync(byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs);
        System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs);
        System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>> CallServiceAsync(Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs);
    }

    public interface IRunServices<TService, TServiceResponse>
    {
        System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> RunAllPreferredGenericServicesAsync(System.Collections.Generic.IEnumerable<ICallServices<TServiceResponse>> runs, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs);
        System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> RunAllPreferredGenericServicesAsync(System.Collections.Generic.IEnumerable<ICallServices<TServiceResponse>> runs, string text, System.Collections.Generic.Dictionary<string, string> apiArgs);
        System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> RunAllPreferredGenericServicesAsync(System.Collections.Generic.IEnumerable<ICallServices<TServiceResponse>> runs, Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs);
    }

    public class CallServices<TService, TTServiceResponse> : RunServices<TService, TTServiceResponse>, ICallServices<TTServiceResponse>
    {
        public Settings.Service service;
        public CallServices(Settings.Service service)
        {
            this.service = service;
            foreach (string argType in new string[] { "binary", "text", "url" })
                CompatibileArgTypesPerProvider.Add(argType, service.requests.Where(r => r.argType == argType).Count());
        }

        public System.Collections.Generic.Dictionary<string, int> CompatibileArgTypesPerProvider { get; set; } = new System.Collections.Generic.Dictionary<string, int>();

        public virtual async System.Threading.Tasks.Task<CallServiceResponse<TTServiceResponse>> CallServiceAsync(byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            return await ((ICallServices<TTServiceResponse>)this).CallServiceAsync(bytes, apiArgs);
        }

        public virtual async System.Threading.Tasks.Task<CallServiceResponse<TTServiceResponse>> CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            return await ((ICallServices<TTServiceResponse>)this).CallServiceAsync(text, apiArgs);
        }

        public virtual async System.Threading.Tasks.Task<CallServiceResponse<TTServiceResponse>> CallServiceAsync(Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            return await ((ICallServices<TTServiceResponse>)this).CallServiceAsync(url, apiArgs);
        }

        async Task<CallServiceResponse<TTServiceResponse>> ICallServices<TTServiceResponse>.CallServiceAsync(byte[] bytes, Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("GenericPrototypeTaskServices: bytes.Length:" + bytes.Length);
            CallServiceResponse<TTServiceResponse> response = new CallServiceResponse<TTServiceResponse>(service);

            foreach (Settings.Request request in service.requests.Where(p => p.argType == "binary"))
            {
                response.Request = request; // init when response object is newed up?
                await HttpMethods.CallApiAsync(response, bytes, apiArgs);
                switch (request.response.type)
                {
                    case "binary":
                        System.IO.File.WriteAllBytes(response.FileName + ".bin", response.ResponseBytes);
                        break;
                    case "json":
                        await HttpMethods.ExtractResultAsync(response);
                        break;
                    default:
                        break;
                }
                return response;
            }
            throw new Exception("GenericServiceAsync: binary: no match for request: type:" + service.classInterface);
        }

        async Task<CallServiceResponse<TTServiceResponse>> ICallServices<TTServiceResponse>.CallServiceAsync(string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("Generic: text:" + text);
            CallServiceResponse<TTServiceResponse> response = new CallServiceResponse<TTServiceResponse>(service);

            foreach (Settings.Request request in service.requests.Where(p => p.argType == "text"))
            {
                response.Request = request; // init when response object is newed up?
                await HttpMethods.CallApiAsync(response, text, apiArgs);
                switch (request.response.type)
                {
                    case "binary":
                        System.IO.File.WriteAllBytes(response.FileName + ".bin", response.ResponseBytes);
                        break;
                    case "json":
                        await HttpMethods.ExtractResultAsync(response);
                        break;
                    default:
                        break;
                }
                return response;
            }
            throw new Exception("GenericServiceAsync: text: no match for request: type:" + service.classInterface);
        }

        async Task<CallServiceResponse<TTServiceResponse>> ICallServices<TTServiceResponse>.CallServiceAsync(Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            Log.WriteLine("GenericServiceAsync: url:" + url);
            CallServiceResponse<TTServiceResponse> response = new CallServiceResponse<TTServiceResponse>(service);

            foreach (Settings.Request request in service.requests.Where(p => p.argType == "url"))
            {
                response.Request = request; // init when response object is newed up?
                await HttpMethods.CallApiAsync(response, url, apiArgs);
                switch (request.response.type)
                {
                    case "binary":
                        System.IO.File.WriteAllBytes(response.FileName + ".bin", response.ResponseBytes);
                        break;
                    case "json":
                        await HttpMethods.ExtractResultAsync(response);
                        break;
                    default:
                        break;
                }
                return response;
            }
            throw new Exception("GenericServiceAsync: url: no match for request: type:" + service.classInterface);
        }

    }


    public class RunServices<TService, TServiceResponse> : IRunServices<TService, TServiceResponse>
    {
        public virtual System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> RunAllPreferredGenericServicesAsync(System.Collections.Generic.IEnumerable<ICallServices<TServiceResponse>> runs, byte[] bytes, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            System.Collections.Generic.List<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> tasks = new System.Collections.Generic.List<Task<CallServiceResponse<TServiceResponse>>>();
            foreach (ICallServices<TServiceResponse> run in runs)
            {
                // TODO: Time consuming puzzle. Never solved. I want to replace dynamic cast with a strong type, how?
                tasks.Add(System.Threading.Tasks.Task.Run(() => ((dynamic)run).CallServiceAsync(bytes, apiArgs)).ContinueWith((c) =>
                {
                    CallServiceResponse<TServiceResponse> r = c.Result.Result;

                    if (r.StatusCode != 200)
                        Console.WriteLine(r.Service.name + " Generic (async): Failed with StatusCode of " + r.StatusCode);
                    else if (r.Request.response.type == "binary")
                    {
                        if (r.ResponseResult != null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseResult not null");
                        if (r.ResponseBytes == null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseBytes is null");
                        Console.WriteLine(r.Service.name + ": Generic (async): response length:" + r.ResponseBytes.Length + ": Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    }
                    else
                    {
                        if (r.ResponseBytes != null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseBytes not null");
                        if (r.ResponseResult == null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseResult is null");
                        Console.WriteLine(r.Service.name + ": Generic (async): ResponseResult:" + r.ResponseResult + ": Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    }

                    return r;
                }));
            }
            return tasks;
        }

        public virtual System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> RunAllPreferredGenericServicesAsync(System.Collections.Generic.IEnumerable<ICallServices<TServiceResponse>> runs, string text, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            System.Collections.Generic.List<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> tasks = new System.Collections.Generic.List<Task<CallServiceResponse<TServiceResponse>>>();
            foreach (ICallServices<TServiceResponse> run in runs)
            {
                // TODO: Time consuming puzzle. Never solved. I want to replace dynamic cast with a strong type, how?
                tasks.Add(System.Threading.Tasks.Task.Run(() => ((dynamic)run).CallServiceAsync(text, apiArgs)).ContinueWith((c) =>
                 {
                     CallServiceResponse<TServiceResponse> r = c.Result.Result;

                     if (r.StatusCode != 200)
                         Console.WriteLine(r.Service.name + " Generic (async): Failed with StatusCode of " + r.StatusCode);
                     else if (r.Request.response.type == "binary")
                     {
                         if (r.ResponseResult != null)
                             throw new Exception("RunAllPreferredGenericServicesRun: ResponseResult not null");
                         if (r.ResponseBytes == null)
                             throw new Exception("RunAllPreferredGenericServicesRun: ResponseBytes is null");
                         Console.WriteLine(r.Service.name + ": Generic (async): response length:" + r.ResponseBytes.Length + ": Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                     }
                     else
                     {
                         if (r.ResponseBytes != null)
                             throw new Exception("RunAllPreferredGenericServicesRun: ResponseBytes not null");
                         if (r.ResponseResult == null)
                             throw new Exception("RunAllPreferredGenericServicesRun: ResponseResult is null");
                         Console.WriteLine(r.Service.name + ": Generic (async): ResponseResult:" + r.ResponseResult + ": Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                     }

                     return r;
                 }));
            }
            return tasks;
        }

        public virtual System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> RunAllPreferredGenericServicesAsync(System.Collections.Generic.IEnumerable<ICallServices<TServiceResponse>> runs, Uri url, System.Collections.Generic.Dictionary<string, string> apiArgs)
        {
            System.Collections.Generic.List<System.Threading.Tasks.Task<CallServiceResponse<TServiceResponse>>> tasks = new System.Collections.Generic.List<Task<CallServiceResponse<TServiceResponse>>>();
            foreach (ICallServices<TServiceResponse> run in runs)
            {
                // TODO: Time consuming puzzle. Never solved. I want to replace dynamic cast with a strong type, how?
                tasks.Add(System.Threading.Tasks.Task.Run(() => ((dynamic)run).CallServiceAsync(url, apiArgs)).ContinueWith((c) =>
                {
                    CallServiceResponse<TServiceResponse> r = c.Result.Result;

                    if (r.StatusCode != 200)
                        Console.WriteLine(r.Service.name + " Generic (async): Failed with StatusCode of " + r.StatusCode);
                    else if (r.Request.response.type == "binary")
                    {
                        if (r.ResponseResult != null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseResult not null");
                        if (r.ResponseBytes == null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseBytes is null");
                        Console.WriteLine(r.Service.name + ": Generic (async): response length:" + r.ResponseBytes.Length + ": Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    }
                    else
                    {
                        if (r.ResponseBytes != null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseBytes not null");
                        if (r.ResponseResult == null)
                            throw new Exception("RunAllPreferredGenericServicesRun: ResponseResult is null");
                        Console.WriteLine(r.Service.name + ": Generic (async): ResponseResult:" + r.ResponseResult + ": Total " + r.TotalElapsedMilliseconds + "ms Request " + r.RequestElapsedMilliseconds + "ms");
                    }

                    return r;
                }));
            }
            return tasks;
        }
    }
}
