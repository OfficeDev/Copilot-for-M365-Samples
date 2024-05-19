export const config = {
  botId: process.env.BOT_ID,
  botPassword: process.env.BOT_PASSWORD,
  storageAccountConnectionString: process.env.STORAGE_ACCOUNT_CONNECTION_STRING
};

// This config is used for Azure Cmmunication Services API calls. Add values from your communication resource in the Azure portal
export const acsConfig = {
  connectionString: "", // Replace with connection string of the communication resource.
  fromPhoneNumber: "", // Replace with the phone number, short code or alphanumeric sender ID configured in the Azure portal for your communication resource.
  fromEmailDomain: "", // Replace with the free email domain generated for your resource.
  whatsAppChannelId: "", // Replace with the WhatsApp channel registration ID.
  whatsAppTemplateName: "", // Create a new WhatsApp template and replace with the template name.
};

export default config;
