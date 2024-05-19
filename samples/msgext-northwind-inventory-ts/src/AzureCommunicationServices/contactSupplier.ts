import { acsConfig } from "../config";
import { EmailClient, EmailMessage, KnownEmailSendStatus } from "@azure/communication-email";
import { SmsClient, SmsSendRequest } from "@azure/communication-sms";
import MessageClient, { MessageTemplateText } from "@azure-rest/communication-messages"

const acsConnectionString = acsConfig.connectionString;

export async function sendEmailMessage(message: string, subject: string, toAddress: string): Promise<void> {
  let emailClient: EmailClient;
  let email: EmailMessage;
  const POLLER_WAIT_TIME = 10
  email = {
    senderAddress: acsConfig.fromEmailDomain,
    content: {
      subject: subject,
      plainText: message,
    },
    recipients: {
      to: [
        { address: toAddress, },
      ],
    },
  };

  emailClient = new EmailClient(acsConnectionString);

  try {
    const poller = await emailClient.beginSend(email);
    if (!poller.getOperationState().isStarted) {
      throw "Poller was not started."
    }

    let timeElapsed = 0;
    while (!poller.isDone()) {
      poller.poll();
      console.log("Email send polling in progress");

      await new Promise(resolve => setTimeout(resolve, POLLER_WAIT_TIME * 100));
      timeElapsed += 10;

      if (timeElapsed > 18 * POLLER_WAIT_TIME) {
        throw "Polling timed out.";
      }
    }

    if (poller.getResult().status === KnownEmailSendStatus.Succeeded) {
      console.log(`📨Successfully sent the email (operation id: ${poller.getResult().id})`);
    }
    else {
      throw poller.getResult().error;
    }
  } catch (e) {
    console.log(`❌ Error: ${e}`);
  }
}

export async function sendSMSMessage(message: string, toNumber: string): Promise<void> {
  const smsClient = new SmsClient(acsConnectionString);
  const sendOptions: SmsSendRequest = {
    from: acsConfig.fromPhoneNumber,
    to: [toNumber],
    message: message
  };

  try {
    const result = await smsClient.send(sendOptions);
    console.log(`📱Successfully sent the SMS`);
  } catch (e) {
    console.log(`❌ Error: ${e}`);
    throw e;
  }
}

// WhatsApp message can be sent using an approved template only. Read the lab exercises for creating a template before using this function.
export async function sendWhatsAppMessage(unitsToOrder: string, product: string, toNumber: string, supplier: string): Promise<void> {

  // Instantiate the client
  const client = MessageClient(acsConnectionString);

  const channelRegistrationId = acsConfig.whatsAppChannelId;

  const recipientList = [toNumber];

  // Create the parameters for the template. These are the values that will be replaced in the template mentioned in the lab file.
  // If you created a different template, make sure to replace the text here to reflect the parameters in your template.
  const supplierName: MessageTemplateText = { name: "user", kind: "text", text: supplier };
  const quantity: MessageTemplateText = { name: "quantity", kind: "text", text: unitsToOrder };
  const productName: MessageTemplateText = { name: "product_name", kind: "text", text: product };

  const parameters = [
    supplierName,
    quantity,
    productName
  ];

  // Assemble the template content
  const template = {
    name: acsConfig.whatsAppTemplateName,
    language: "en_us",
    values: parameters,
    bindings: {
      kind: "whatsApp",
      body: [
        {
          refValue: "user",
        },
        {
          refValue: "quantity",
        },
        {
          refValue: "product_name",
        }
      ]
    },
  };

  // Send template message
  const templateMessageResult = await client.path("/messages/notifications:send").post({
    contentType: "application/json",
    body: {
      channelRegistrationId: channelRegistrationId,
      to: recipientList,
      kind: "template",
      template: template
    }
  });

  // Process result
  if (templateMessageResult.status === "202") {
    console.log("📗Successfully sent the WhatsApp message");
  } else {
    throw new Error("`❌ Failed to send message");
  }
}