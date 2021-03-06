namespace Saturn

open Microsoft.AspNetCore.Http
open Giraffe.HttpStatusCodeHandlers
open Giraffe.Core
open Giraffe.ResponseWriters
open Giraffe.ModelBinding
open FSharp.Control.Tasks.ContextInsensitive

[<AutoOpen>]
module ControllerHelpers =

  module Controller =

    ///Returns to the client content serialized to JSON.
    let json (ctx: HttpContext) (obj: 'a)   =
      ctx.WriteJsonAsync(obj)

    ///Returns to the client content serialized to JSON. Accepts custom serialization settings
    let jsonCustom (ctx: HttpContext) settings obj=
      ctx.WriteJsonAsync(settings, obj)

    ///Returns to the client content serialized to XML.
    let xml (ctx: HttpContext) (obj: 'a) =
      ctx.WriteXmlAsync obj

    ///Returns to the client content as string.
    let text (ctx: HttpContext) (value: string) =
      ctx.WriteTextAsync value

    ///Returns to the client rendered template.
    let render (ctx: HttpContext) template =
      ctx.WriteHtmlStringAsync template

    ///Returns to the client rendered xml template.
    let renderXml (ctx: HttpContext) template =
      ctx.WriteHtmlStringAsync (Giraffe.GiraffeViewEngine.renderXmlNode template)

    ///Returns to the client static file.
    let file (ctx: HttpContext) path =
      ctx.WriteHtmlFileAsync path

    ///Gets model from body as JSON.
    let getJson<'a> (ctx: HttpContext) =
      ctx.BindJsonAsync<'a>()

    ///Gets model from body as XML.
    let getXml<'a> (ctx: HttpContext) =
      ctx.BindXmlAsync<'a>()

    ///Gets model from urelencoded body.
    let getForm<'a> (ctx : HttpContext) =
      ctx.BindFormAsync<'a>()

    ///Gets model from urelencoded body. Accepts culture name
    let getFormCulture<'a> (ctx: HttpContext) culture =
      let clt = System.Globalization.CultureInfo.CreateSpecificCulture culture
      ctx.BindFormAsync<'a> clt

    ///Gets model from query string.
    let getQuery<'a> (ctx : HttpContext) =
      ctx.BindQueryString<'a>()

    ///Gets model from query string. Accepts culture name
    let getQueryCulture<'a> (ctx: HttpContext) culture =
      let clt = System.Globalization.CultureInfo.CreateSpecificCulture culture
      ctx.BindQueryString<'a> clt

    ///Get model based on `HttpMethod` and `Content-Type` of request.
    let getModel<'a> (ctx: HttpContext) =
      match ctx.Items.TryGetValue "RequestModel" with
      | true, o -> task { return unbox<'a> o }
      | _ ->
        ctx.BindModelAsync<'a>()

    ///Get model based on `HttpMethod` and `Content-Type` of request. Accepts custom culture.
    let getModelCustom<'a> (ctx: HttpContext) culture =
      let clt = culture |> Option.map System.Globalization.CultureInfo.CreateSpecificCulture
      match clt with
      | Some c -> ctx.BindModelAsync<'a>(c)
      | None -> ctx.BindModelAsync<'a>()

    ///Loads model populated by `fetchModel` pipeline
    let loadModel<'a> (ctx: HttpContext) =
      match ctx.Items.TryGetValue "RequestModel" with
      | true, o -> Some (unbox<'a> o)
      | _ -> None

    ///Gets path of the request - it's relative to current `scope`
    let getPath (ctx: HttpContext) =
      ctx.Request.Path.Value

    ///Gets url of the request
    let getUrl (ctx: HttpContext) =
      match ctx.Items.TryGetValue "RequestUrl" with
      | true, o -> Some (unbox<string> o)
      | _ -> None

    ///Gets the contents of the `Configuration` key in the HttpContext dictionary, unboxed as the given type.
    let getConfig<'a> (ctx: HttpContext) =
      unbox<'a> ctx.Items.["Configuration"]

    ///Sends the contents of a file as the body of the response. Does not set a Content-Type.
    let sendDownload (ctx: HttpContext) (path: string) =
      let cnt = System.IO.File.ReadAllBytes path
      setBody cnt  Common.halt ctx

    ///Send bytes as the body of the response. Does not set a Content-Type.
    let sendDownloadBinary (ctx: HttpContext) (content: byte []) =
      setBody content Common.halt ctx

    ///Perform a temporary redirect to the provided location.
    let redirect (ctx: HttpContext) path =
      redirectTo false path Common.halt ctx

  /// This module wraps Giraffe responses (ie setting HTTP status codes) for easy chaining in the Saturn model.
  /// All of the functions set the status code and halt further processing.
  module Response =
    let ``continue`` (ctx: HttpContext) =
      Intermediate.CONTINUE Common.halt ctx

    let switchingProto (ctx: HttpContext) =
      Intermediate.SWITCHING_PROTO Common.halt ctx

    let ok (ctx: HttpContext) res =
      Successful.OK res Common.halt ctx

    let created (ctx: HttpContext) res =
      Successful.CREATED res Common.halt ctx

    let accepted (ctx: HttpContext) res =
      Successful.ACCEPTED res Common.halt ctx

    let badRequest (ctx: HttpContext) res =
      RequestErrors.BAD_REQUEST res Common.halt ctx

    let unauthorized (ctx: HttpContext) scheme realm res =
      RequestErrors.UNAUTHORIZED scheme realm res Common.halt ctx

    let forbidden (ctx: HttpContext) res =
      RequestErrors.FORBIDDEN res Common.halt ctx

    let notFound (ctx: HttpContext) res =
      RequestErrors.NOT_FOUND res Common.halt ctx

    let methodNotAllowed (ctx: HttpContext) res =
      RequestErrors.METHOD_NOT_ALLOWED res Common.halt ctx

    let notAcceptable (ctx: HttpContext) res =
      RequestErrors.NOT_ACCEPTABLE res Common.halt ctx

    let conflict (ctx: HttpContext) res =
      RequestErrors.CONFLICT res Common.halt ctx

    let gone (ctx: HttpContext) res =
      RequestErrors.GONE res Common.halt ctx

    let unuspportedMediaType (ctx: HttpContext) res =
      RequestErrors.UNSUPPORTED_MEDIA_TYPE res Common.halt ctx

    let unprocessableEntity (ctx: HttpContext) res =
      RequestErrors.UNPROCESSABLE_ENTITY res Common.halt ctx

    let preconditionRequired (ctx: HttpContext) res =
      RequestErrors.PRECONDITION_REQUIRED res Common.halt ctx

    let tooManyRequests (ctx: HttpContext) res =
      RequestErrors.TOO_MANY_REQUESTS res Common.halt ctx

    let internalError (ctx: HttpContext) res =
      ServerErrors.INTERNAL_ERROR res Common.halt ctx

    let notImplemented (ctx: HttpContext) res =
      ServerErrors.NOT_IMPLEMENTED res Common.halt ctx

    let badGateway (ctx: HttpContext) res =
      ServerErrors.BAD_GATEWAY res Common.halt ctx

    let serviceUnavailable (ctx: HttpContext) res =
      ServerErrors.SERVICE_UNAVAILABLE res Common.halt ctx

    let gatewayTimeout (ctx: HttpContext) res =
      ServerErrors.GATEWAY_TIMEOUT res Common.halt ctx