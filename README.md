# Office365 Video API using plain REST

This sample demonstrates how to use the Office365 Video API using plain REST protocol without any dependence on third party libraries.

## Getting Started
1. Create Office 365 Developer Account.
	In order to do anything, we first have to have an Office365 developer account, make sure you complete the steps to associate your Azure subscription to the Office 365 account. [https://msdn.microsoft.com/office/office365/HowTo/setup-development-environment#bk_CreateAzureSubscription](https://msdn.microsoft.com/office/office365/HowTo/setup-development-environment#bk_CreateAzureSubscription)
2. Create a new Azure Active Directory application: [https://msdn.microsoft.com/office/office365/HowTo/add-common-consent-manually](https://msdn.microsoft.com/office/office365/HowTo/add-common-consent-manually)
	1. When adding permissions for Office 365 SharePoint Online, select "Read and Write User Files" and "Read and write items and lists in all site collections":
	![](http://i.imgur.com/cqDhxrO.png)
	2. Set application to be multi-tenant:
	![](http://i.imgur.com/9eX489J.png)
	3. Make certain you copy and store the key value

## Modify the project to run
1. Add your own videos to the o365Sample\Content\SampleVideos directory.
2. Modify the web.config file with your Azure AD Client ID and Client Secret

## Known Issues
1. The sample currently fails at the point when it attempts to create a placeholder for the video file. This is still a work in progress.

## Useful Resources

* Set up your Office 365 development environment - [https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment](https://msdn.microsoft.com/en-us/office/office365/howto/setup-development-environment)
* Office 365 app authentication concepts - [https://msdn.microsoft.com/office/office365/HowTo/common-app-authentication-tasks](https://msdn.microsoft.com/office/office365/HowTo/common-app-authentication-tasks)
* Manually register your app with Azure AD so it can access Office 365 APIs - [https://msdn.microsoft.com/office/office365/HowTo/add-common-consent-manually](https://msdn.microsoft.com/office/office365/HowTo/add-common-consent-manually)
* Office 365 API reference - [https://msdn.microsoft.com/en-us/office/office365/api/api-catalog](https://msdn.microsoft.com/en-us/office/office365/api/api-catalog)
* API Sandbox - [https://apisandbox.msdn.microsoft.com/](https://apisandbox.msdn.microsoft.com/)
* Working with Office 365 APIs – The RAW Version - [http://chakkaradeep.com/index.php/working-with-office365apis-the-raw-version/](http://chakkaradeep.com/index.php/working-with-office365apis-the-raw-version/)
