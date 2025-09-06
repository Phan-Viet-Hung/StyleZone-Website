using API.DomainCusTomer.DTOs.MoMo;
using API.DomainCusTomer.Services.IServices;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using RestSharp;
using System.Security.Cryptography;
using System.Text;

public class MomoServicer : IMomoService
{
    private readonly IOptions<MomoOptionModel> _options;

    public MomoServicer(IOptions<MomoOptionModel> options)
    {
        _options = options;
    }

    public async Task<MomoCreatePaymentResponseModel> CreatePaymentAsync(OrderInfoModel model)
    {
        model.OrderId = DateTime.UtcNow.Ticks.ToString();
        model.OrderInfo = $"Khách hàng: {model.FullName}. Nội dung: {model.OrderInfo}";

        var rawData =
            $"partnerCode={_options.Value.PartnerCode}" +
            $"&accessKey={_options.Value.AccessKey}" +
            $"&requestId={model.OrderId}" +
            $"&amount={model.Amount}" +
            $"&orderId={model.OrderId}" +
            $"&orderInfo={model.OrderInfo}" +
            $"&returnUrl={_options.Value.ReturnUrl}" +
            $"&notifyUrl={_options.Value.NotifyUrl}" +
            $"&extraData=";

        var signature = ComputeHmacSha256(rawData, _options.Value.SecretKey);

        var client = new RestClient(_options.Value.MomoApiUrl);
        var request = new RestRequest() { Method = Method.Post };

        request.AddHeader("Content-Type", "application/json; charset=UTF-8");

        var requestData = new
        {
            accessKey = _options.Value.AccessKey,
            partnerCode = _options.Value.PartnerCode,
            requestType = _options.Value.RequestType,
            notifyUrl = _options.Value.NotifyUrl,
            returnUrl = _options.Value.ReturnUrl,
            orderId = model.OrderId,
            amount = model.Amount.ToString(),
            orderInfo = model.OrderInfo,
            requestId = model.OrderId,
            extraData = "",
            signature = signature
        };

        request.AddJsonBody(requestData);
        var response = await client.ExecuteAsync(request);
        return JsonConvert.DeserializeObject<MomoCreatePaymentResponseModel>(response.Content);
    }

    public MomoExecuteResponseModel PaymentExecuteAsync(IQueryCollection collection)
    {
        return ExtractResultFromCollection(collection.ToDictionary(x => x.Key, x => x.Value.ToString()));
    }

    public MomoExecuteResponseModel PaymentExecuteAsync(IFormCollection collection)
    {
        return ExtractResultFromCollection(collection.ToDictionary(x => x.Key, x => x.Value.ToString()));
    }

    private MomoExecuteResponseModel ExtractResultFromCollection(Dictionary<string, string> data)
    {
        data.TryGetValue("amount", out var amount);
        data.TryGetValue("orderInfo", out var orderInfo);
        data.TryGetValue("orderId", out var orderId);
        data.TryGetValue("errorCode", out var errorCode);
        data.TryGetValue("message", out var message);
        data.TryGetValue("localMessage", out var localMessage);
        data.TryGetValue("transId", out var transId);
        data.TryGetValue("signature", out var signature);
        data.TryGetValue("resultCode", out var resultCode);

        return new MomoExecuteResponseModel
        {
            Amount = amount,
            OrderId = orderId,
            OrderInfo = orderInfo,
            ErrorCode = errorCode,
            Message = message,
            LocalMessage = localMessage,
            TransId = transId,
            Signature = signature,
            ResultCode = resultCode
        };
    }

    public bool ValidateSignature(IQueryCollection collection)
    {
        return ValidateSignature(collection.ToDictionary(x => x.Key, x => x.Value.ToString()));
    }

    public bool ValidateSignature(IFormCollection collection)
    {
        return ValidateSignature(collection.ToDictionary(x => x.Key, x => x.Value.ToString()));
    }

    private bool ValidateSignature(Dictionary<string, string> data)
    {
        if (!data.TryGetValue("signature", out var signature))
            return false;

        var requiredFields = new[]
        {
            "partnerCode", "accessKey", "requestId", "amount", "orderId", "orderInfo", "orderType",
            "transId", "message", "localMessage", "responseTime", "errorCode", "payType", "extraData"
        };

        foreach (var field in requiredFields)
        {
            if (!data.ContainsKey(field))
                data[field] = ""; // Dự phòng để không bị lỗi thiếu key
        }

        var rawData =
            $"partnerCode={data["partnerCode"]}" +
            $"&accessKey={_options.Value.AccessKey}" +
            $"&requestId={data["requestId"]}" +
            $"&amount={data["amount"]}" +
            $"&orderId={data["orderId"]}" +
            $"&orderInfo={data["orderInfo"]}" +
            $"&orderType={data["orderType"]}" +
            $"&transId={data["transId"]}" +
            $"&message={data["message"]}" +
            $"&localMessage={data["localMessage"]}" +
            $"&responseTime={data["responseTime"]}" +
            $"&errorCode={data["errorCode"]}" +
            $"&payType={data["payType"]}" +
            $"&extraData={data["extraData"]}";

        var computedSignature = ComputeHmacSha256(rawData, _options.Value.SecretKey);
        return computedSignature == signature;
    }

    private string ComputeHmacSha256(string message, string secretKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secretKey);
        var messageBytes = Encoding.UTF8.GetBytes(message);

        using (var hmac = new HMACSHA256(keyBytes))
        {
            var hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }
}
