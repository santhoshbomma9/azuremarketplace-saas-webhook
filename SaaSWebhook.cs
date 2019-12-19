using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using SendGrid.Helpers.Mail;

namespace azuremarketplace_saas_webhook
{
    public static class SaaSWebhook
    {
        [FunctionName("WebhookSendEmail")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            [SendGrid(ApiKey = "AzureWebJobsSendGridApiKey")] IAsyncCollector<SendGridMessage> messageCollector,
            [Table("Webhookoperations", Connection = "MyStorage")] IAsyncCollector<WebHookNotification> webHookNotification,
            ILogger log)
        {
            // Get request body and create as a table object 
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var notificationData = JsonConvert.DeserializeObject<WebHookNotification>(requestBody);
            notificationData.PartitionKey = notificationData.PlanId;
            notificationData.RowKey = notificationData.TimeStamp;

            // Log information
            log.LogInformation($"Recevied new WebHook Event for {notificationData.SubscriptionId}");

            // Send email 
            var message = new SendGridMessage();
            message.AddTo("santhoshgoud21984@gmail.com");
            message.AddContent("text/html", notificationData.ToString());
            message.SetFrom(new EmailAddress("santhoshgoud21984@gmail.com"));
            message.SetSubject($"Webhook notification for Subscription {notificationData.SubscriptionId}");
            await messageCollector.AddAsync(message);
            
            // Save to Table storage
            await webHookNotification.AddAsync(notificationData);
            
            return new OkResult();
        }

        public class WebHookNotification
        {
            public string PartitionKey { get; set; }
            public string RowKey { get; set; }
            public string Id { get; set; }
            public string ActivityId { get; set; }
            public string SubscriptionId { get; set; }
            public string PublisherId { get; set; }
            public string OfferId { get; set; }
            public string PlanId { get; set; }
            public string Quantity { get; set; }
            public string TimeStamp { get; set; }
            public string Action { get; set; }
            public string Status { get; set; }

            public override string ToString()
            {
                return $" {Id}  {ActivityId}  {SubscriptionId}  {PublisherId}  {OfferId}  {PlanId}  {Quantity}  {TimeStamp}  {Action}  {Status} ";
            }
        }
    }
}
