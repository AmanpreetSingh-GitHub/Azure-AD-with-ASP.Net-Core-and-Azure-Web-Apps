# Integrating Azure Active Directory authentication in ASP.NET Core Web Application using Azure WebApps #

### Introduction ###

This article covers following things –

- Creating FrameworkThree framework using <a href="https://github.com/AmanpreetSingh-GitHub/Architecture-FrameworkTwo">FrameworkTwo</a>
 framework (n-layered Architecture using ASP.NET Core Web Application, Web API, Entity Framework Core, Generic Repository and Unit-of-Work)
- Adding Save functionality to the framework
- Integrating Azure Active Directory authentication in the framework
	- Registering new Apps with Azure AD for FrameworkThree.Service and FrameworkThree.Web projects
	- Configuring FrameworkThree solution to work with Azure AD Apps for authentication

### Create new FrameworkThree solution ###

- Create new solution FrameworkThree following steps from <a href="https://github.com/AmanpreetSingh-GitHub/Architecture-FrameworkTwo">FrameworkTwo</a>

### Changes to FrameworkThree solution ###

##### FrameworkThree.BusinessLayer changes #####

- Add Common folder in FrameworkThree.BusinessLayer

	![](Images/1.PNG)

- Add new class ResponseMessage.cs to Common folder and add below code

	>     namespace FrameworkThree.BusinessLayer.Common
	>     {
	>         public class ResponseMessage<T> where T : class
	>         {
	>             public Exception Exception { get; set; }
	>     
	>             public int Total { get; set; }
	>     
	>             public T Result { get; set; }
	>     
	>             public string StatusText { get; set; }
	>     
	>             public List<string> Messages { get; set; }
	>     
	>             public HttpStatusCode StatusCode { get; set; }
	>     
	>             public bool Success
	>             {
	>                 get
	>                 {
	>                     return StatusCode == HttpStatusCode.OK;
	>                 }
	>             }
	>     
	>             public bool Unauthorized
	>             {
	>                 get
	>                 {
	>                     return StatusCode == HttpStatusCode.Unauthorized;
	>                 }
	>             }
	>     
	>             public void SetAsBadRequest()
	>             {
	>                 StatusCode = HttpStatusCode.BadRequest;
	>             }
	>     
	>             public void SetAsGoodRequest()
	>             {
	>                 StatusCode = HttpStatusCode.OK;
	>             }
	>         }
	>     }

##### FrameworkThree.Service changes #####

- Update StudentController.cs to return ResponseMessage

	>     // GET api/employee
	>     [HttpGet]
	>     public ResponseMessage<List<StudentModel>> Get()
	>     {
	>         ResponseMessage<List<StudentModel>> response = new ResponseMessage<List<StudentModel>>();
	>         response.Result = studentLogic.GetStudents();
	>     
	>         return response;
	>     }

##### FrameworkThree.Web changes #####

- In ServiceInterface.cs -> GetDataAsync method update below statement as below -
	- Old
		>     output.Result = JsonConvert.DeserializeObject<T>(responseString);
	- Updated
		>     output = JsonConvert.DeserializeObject<RestMessage<T>>(responseString);      

- Update menus in _Layout.cshtml file as below -

	>     <div class="navbar-collapse collapse">
	>         <ul class="nav navbar-nav">
	>             <li><a asp-area="" asp-controller="Student" asp-action="Index">Student Area</a></li>
	>         </ul>
	>     </div>

- In StudentController.cs move Index method code to new method GetStudents and update Index method as below -

	>     public IActionResult Index()
	>     {
	>         return View();
	>     }
	>     
	>     public async Task<IActionResult> GetStudents()
	>     {
	>         RestMessage<List<StudentModel>> response = new RestMessage<List<StudentModel>>();
	>     
	>         try
	>         {
	>             ServiceInterface serviceInterface = ServiceInterface.Instance;
	>     
	>             response = await serviceInterface.GetDataAsync<List<StudentModel>>("student");
	>     
	>             if (!response.Success)
	>             {
	>                 response.StatusText = "Error fetching Student data";
	>             }
	>         }
	>         catch (Exception e)
	>         {
	>             response.Exception = e;
	>             response.SetAsBadRequest();
	>             response.StatusText = "Error fetching Student data";
	>         }
	>     
	>         return Json(response);
	>     }

- Update Views -> Student -> Index.cshtml file as below -

	>     <div style="margin-top:20px;">
	>         <a href="Student/GetStudents" class="btn btn-primary btn-sm">Get Students</a>     
	>     </div>


### Adding Save functionality to the solution ###

#### FrameworkThree.BusinessLayer ####

- In IStudentLogic.cs add below method definition -

	>     StudentModel SaveStudent(StudentModel studentModel);

- In StudentLogic.cs add code for SaveStudent method -

	>     public StudentModel SaveStudent(StudentModel studentModel)
	>     {
	>         Student student = ConvertStudentModelToStudent(studentModel);
	>         studentRepository.Save(student);
	>         this.unitOfWork.Save();
	>     
	>         return studentModel;
	>     }
	>     
	>     private Student ConvertStudentModelToStudent(StudentModel studentModel)
	>     {
	>         Student student = new Student()
	>         {
	>             StudentId = studentModel.StudentId,
	>             Name = studentModel.Name
	>         };
	>     
	>         return student;
	>     }

#### FrameworkThree.Service ####

- Update Post method code as below -

	>     // POST api/values
	>     [HttpPost]
	>     public ResponseMessage<StudentModel> Post([FromBody]StudentModel studentModel)
	>     {
	>         ResponseMessage<StudentModel> response = new ResponseMessage<StudentModel>();
	>         response.Result = studentLogic.SaveStudent(studentModel);
	>     
	>         return response;
	>     }

#### FrameworkThree.Web ####

- Add PostDataToAPI method in ServiceInterface.cs

	>     public async Task<RestMessage<T>> PostDataToAPI<T>(string controller, object content) where T : class
	>     {
	>         RestMessage<T> output = new RestMessage<T>();
	>     
	>         try
	>         {
	>             string contentData = JsonConvert.SerializeObject(content);
	>     
	>             HttpClient client = new HttpClient();
	>             HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, baseServiceURL + "/api/" + controller);
	>             request.Content = new StringContent(contentData, Encoding.UTF8, "application/json");
	>             HttpResponseMessage response = await client.SendAsync(request);
	>     
	>             if (response.IsSuccessStatusCode)
	>             {
	>                 var responseString = await response.Content.ReadAsStringAsync();
	>                 output = JsonConvert.DeserializeObject<RestMessage<T>>(responseString);
	>     
	>                 output.SetAsGoodRequest();
	>             }
	>             else
	>             {
	>                 output.SetAsBadRequest();
	>                 output.Exception = new Exception("Error occured during processing");
	>             }
	>         }
	>         catch (Exception e)
	>         {
	>             output.Exception = e;
	>             output.SetAsBadRequest();
	>             output.StatusText = "Error occured during processing";
	>         }
	>     
	>         return output;
	>     }

- In StudentController.cs add PostStudentData method -

	>     public async Task<IActionResult> PostStudentData(StudentModel studentModel)
	>     {
	>         RestMessage<StudentModel> response = new RestMessage<StudentModel>();
	>     
	>         try
	>         {
	>             ServiceInterface serviceInterface = ServiceInterface.Instance;
	>     
	>             response = await serviceInterface.PostDataToAPI<StudentModel>("student", studentModel);
	>     
	>             if (!response.Success)
	>             {
	>                 response.StatusText = "Error saving data";
	>             }
	>         }
	>         catch (Exception e)
	>         {
	>             response.Exception = e;
	>             response.SetAsBadRequest();
	>             response.StatusText = "Error saving data";
	>         }
	>     
	>         return Json(response);
	>     }

- Also add SaveStudent method in StudentController.cs

	>     public async Task SaveStudent()
	>     {
	>         StudentModel studentModel = new StudentModel()
	>         {
	>             StudentId = 100004,
	>             Name = "Student Four"
	>         };
	>     
	>         await PostStudentData(studentModel);
	>     }

- Update Views -> Student -> Index.cshtml file as below -

	>     <div style="margin-top:20px;">
	>         <a href="Student/GetStudents" class="btn btn-primary btn-sm">Get Students</a>
	>         <a href="Student/SaveStudent" class="btn btn-primary btn-sm">Add Student</a>     
	>     </div>

- Build and test the code for fetching Students and saving new Student

### Integrating Azure Active Directory authentication in the framework ###

#### Registering new Apps with Azure AD ####

- Login to your <a href="http://portal.azure.com">Azure portal</a>

	<img src="Images/2.PNG" width="80%">

- Select *Azure Active Directory* from left to register our Apps 

	<img src="Images/3.PNG" width="80%">

- Click on *App registration* to register FrameworkThree.Service and FrameworkThree.Web

	<img src="Images/4.PNG" width="80%">

##### Registering new App for FrameworkThree.Service project #####

- Enable SSL by checking *Enable SSL* checkbox

	<img src="Images/5.PNG" width="80%">

- Create new *Web app / API* type application for FrameworkThree.Service project. Copy SSL path from previous image and put in *Sign-on URL*

	<img src="Images/6.PNG" width="30%">

- Select *Properties* of FrameworkThreeService created in previous step and update *App ID URI* to https://*&lt;&lt;tenantname&gt;&gt;*/FrameworkThreeService (Replace *tenantname* with tenant name of your Azure AD)

	<img src="Images/7.PNG" width="80%">
	<img src="Images/8.PNG" width="30%">

##### Registering new App for FrameworkThree.Web project #####

- Enable SSL by checking *Enable SSL* checkbox

	<img src="Images/9.PNG" width="80%">

- Create new *Web app / API* type application for FrameworkThree.Web project. Copy SSL path from previous image and put in *Sign-on URL*

	<img src="Images/10.PNG" width="30%">

- Select *Properties* of FrameworkThreeWebApp created in previous step and update *Logout URL* to https&#58;//localhost:44370/Account/SessionFinish (https&#58;//localhost:44370 is the SSL URL for FrameworkThree.Web project)

	<img src="Images/11.PNG" width="80%">
	<img src="Images/12.PNG" width="30%">

- Update the *Reply URLs*

	<img src="Images/32.PNG" width="80%">

- Create new Key and preserve it (this will be *Appliction Key* to be used in FrameworkThree.Web project)

	<img src="Images/13.PNG" width="80%">
	<img src="Images/14.PNG" width="80%">

- Grant permission to access FrameworkThreeService App from FrameworkThreeWebApp

	<img src="Images/15.PNG" width="80%">

- Click *Select an  API* and search *FrameworkThreeService*

	<img src="Images/16.PNG" width="80%">
	<img src="Images/17.PNG" width="50%">

- In *Select permissions* select *Access FrameworkThreeService*

	<img src="Images/18.PNG" width="80%">
	<img src="Images/19.PNG" width="80%">

#### Configuring FrameworkThree solution to work with Azure AD Apps for authentication ####

##### Configuring FrameworkThree.Service project to work with Azure AD FrameworkThreeService App #####

- Add Microsoft.IdentityModel.Clients.ActiveDirectory Nuget package

	<img src="Images/22.PNG" width="80%">

- Add Microsoft.AspNetCore.Authentication.JwtBearer Nuget package. This is used to enable the application to receive OpenID Connect Bearer token

	<img src="Images/23.PNG" width="80%">

- Open appsettings.json and add AzureActiveDirectory settings (Replace &lt;&lt;tenantname&gt;&gt; with tenant name of your Azure AD) 

	>     "AzureActiveDirectory": {
	>     	"AuthenticationAuthority": "https://login.microsoftonline.com/<<tenantname>>",
	>     	"AppIDURI": "https://<<tenantname>>/FrameworkThreeService"
	>     }

- Open Startup.cs file and add below code in ConfigureServices method to add Authentication services to the project

	>     services.AddAuthentication();

- Open Startup.cs file and add below code in Configure method. This is used to configure JWT Bearer Authentication in HTTP request pipeline

	>     app.UseJwtBearerAuthentication(new JwtBearerOptions
	>     {
	>         Authority = Configuration["AzureActiveDirectory:AuthenticationAuthority"],
	>         Audience = Configuration["AzureActiveDirectory:AppIDURI"],
	>         AutomaticAuthenticate = true,
	>         AutomaticChallenge = true
	>     });

- Add [Authorize] attribute on StudentController.cs

##### Configuring FrameworkThree.Web project to work with Azure AD FrameworkThreeWebApp App #####

- Add Microsoft.IdentityModel.Clients.ActiveDirectory Nuget package

	<img src="Images/24.PNG" width="80%">

- Add Microsoft.AspNetCore.Session Nuget package to include ASP.NET Core session state middleware in our project

	<img src="Images/25.PNG" width="80%">

- Add Microsoft.AspNetCore.Authentication.Cookies Nuget package for using cookie based authentication in our project

	<img src="Images/26.PNG" width="80%">

- Add Microsoft.AspNetCore.Authentication.OpenIdConnect Nuget package for using OpenID Connect authentication in our project

	<img src="Images/27.PNG" width="80%">

- Open appsettings.json and add AzureActiveDirectory settings
	- Replace &lt;&lt;applicationID&gt;&gt; with *Application ID* of FrameworkThreeWebApp
	- Replace &lt;&lt;applicationKey&gt;&gt; with the *key* added for FrameworkThreeWebApp 
	- Replace &lt;&lt;tenantname&gt;&gt; with tenant name of your Azure AD

	>     "AzureActiveDirectory": {
	>     	"GraphURI": "https://graph.windows.net",
	>     	"AuthenticationAuthority": "https://login.microsoftonline.com/<<tenantname>>",
	>     	"ServiceAppIDURI": "https://<<tenantname>>/FrameworkThreeService",
	>     	"ApplicationID": <<applicationID>>,
	>     	"ApplicationKey": <<applicationKey>>,
	>     	"AfterLogoutRedirectURI": "https://localhost:44370/"
	>     }

- Add a new class in Utils folder names NaiveSessionCache.cs

	<img src="Images/33.PNG" width="80%">

	>     namespace FrameworkThree.Web.Utils
	>     {
	>         public class NaiveSessionCache : TokenCache
	>         {
	>             private static readonly object FileLock = new object();
	>             string UserObjectId = string.Empty;
	>             string CacheId = string.Empty;
	>             ISession Session = null;
	>     
	>             public NaiveSessionCache(string userId, ISession session)
	>             {
	>                 UserObjectId = userId;
	>                 CacheId = UserObjectId + "_TokenCache";
	>                 Session = session;
	>                 this.AfterAccess = AfterAccessNotification;
	>                 this.BeforeAccess = BeforeAccessNotification;
	>                 Load();
	>             }
	>     
	>             public void Load()
	>             {
	>                 lock (FileLock)
	>                 {
	>                     this.Deserialize(Session.Get(CacheId));
	>                 }
	>             }
	>     
	>             public void Persist()
	>             {
	>                 lock (FileLock)
	>                 {
	>                     // reflect changes in the persistent store
	>                     Session.Set(CacheId, this.Serialize());
	>                     // once the write operation took place, restore the HasStateChanged bit to false
	>                     this.HasStateChanged = false;
	>                 }
	>             }
	>     
	>             // Empties the persistent store.
	>             public override void Clear()
	>             {
	>                 base.Clear();
	>                 Session.Remove(CacheId);
	>             }
	>     
	>             public override void DeleteItem(TokenCacheItem item)
	>             {
	>                 base.DeleteItem(item);
	>                 Persist();
	>             }
	>     
	>             // Triggered right before ADAL needs to access the cache.
	>             // Reload the cache from the persistent store in case it changed since the last access.
	>             void BeforeAccessNotification(TokenCacheNotificationArgs args)
	>             {
	>                 Load();
	>             }
	>     
	>             // Triggered right after ADAL accessed the cache.
	>             void AfterAccessNotification(TokenCacheNotificationArgs args)
	>             {
	>                 // if the access operation resulted in a cache update
	>                 if (this.HasStateChanged)
	>                 {
	>                     Persist();
	>                 }
	>             }
	>         }
	>     }

- Open Startup.cs file and add below code in ConfigureServices method
	- Add session services as we will be using session to store token (NaiveSessionCache)
	
		>     services.AddSession();

	- Add Authentication services as we will be using cookie authenticaiton

		>     services.AddAuthentication(sharedOptions => sharedOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme);

- Open Startup.cs file and add below code in Configure method
	- Configure session middleware

		>     app.UseSession();	

	- Populate AzureActiveDirectory configuration values from appsetting.json file

		>     GraphURI = Configuration["AzureActiveDirectory:GraphURI"];
		>     AuthenticationAuthority = Configuration["AzureActiveDirectory:AuthenticationAuthority"];
		>     ServiceAppIDURI = Configuration["AzureActiveDirectory:ServiceAppIDURI"];
		>     ApplicationID = Configuration["AzureActiveDirectory:ApplicationID"];
		>     ApplicationKey = Configuration["AzureActiveDirectory:ApplicationKey"];
		>     AfterLogoutRedirectURI = Configuration["AzureActiveDirectory:AfterLogoutRedirectURI"];

		- For this you will have to add variables at the top

		>     public static string GraphURI;
		>     public static string AuthenticationAuthority;
		>     public static string ServiceAppIDURI;
		>     public static string ApplicationID;
		>     public static string ApplicationKey;
		>     public static string AfterLogoutRedirectURI;

	- Configure the OWIN pipeline to use cookie authentication
	
		>     app.UseCookieAuthentication(new CookieAuthenticationOptions());

	- Configure the OWIN pipeline to use OpenID Connect authentication  

		>     app.UseOpenIdConnectAuthentication(new OpenIdConnectOptions
		>     {
		>         Authority = AuthenticationAuthority,
		>         ClientId = ApplicationID,                
		>         ResponseType = OpenIdConnectResponseType.CodeIdToken,
		>         GetClaimsFromUserInfoEndpoint = false,
		>         PostLogoutRedirectUri = AfterLogoutRedirectURI,
		>     
		>         Events = new OpenIdConnectEvents
		>         {
		>             OnRemoteFailure = OnRemoteFailureHandler,
		>             OnAuthorizationCodeReceived = OnAuthorizationCodeReceivedHandler,
		>         }
		>     });

	- Add OnAuthorizationCodeReceivedHandler method to run code after authentication

		>     private async Task OnAuthorizationCodeReceivedHandler(AuthorizationCodeReceivedContext context)
		>     {
		>         Uri redirectURI = new Uri(context.Properties.Items[OpenIdConnectDefaults.RedirectUriForCodePropertiesKey]);
		>         string azureADUserObjectID = (context.Ticket.Principal.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
		>     
		>         ClientCredential clientCredential = new ClientCredential(ApplicationID, ApplicationKey);
		>         AuthenticationContext authenticationContext = new AuthenticationContext(AuthenticationAuthority, new NaiveSessionCache(azureADUserObjectID, context.HttpContext.Session));
		>         AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenByAuthorizationCodeAsync(context.ProtocolMessage.Code, redirectURI, clientCredential, GraphURI);
		>     
		>         context.HandleCodeRedemption(authenticationResult.AccessToken, authenticationResult.IdToken);
		>     }

	- Add OnRemoteFailureHandler method to handle sign-in errors

		>     private Task OnAuthenticationFailed(FailureContext context)
		>     {
		>         context.HandleResponse();
		>         
		>         context.Response.Redirect("/Home/Error?message=" + context.Failure.Message);
		>         
		>         return Task.FromResult(0);
		>     }

- Add new controller AccountController.cs and add Signin, Signout, SessionFinish methods

	<img src="Images/28.PNG" width="80%">

	>     namespace FrameworkThree.Web.Controllers
	>     {
	>         public class AccountController : Controller
	>         {
	>             [HttpGet]
	>             public async Task Signin()
	>             {
	>                 if (HttpContext.User == null || !HttpContext.User.Identity.IsAuthenticated)
	>                 {
	>                     await HttpContext.Authentication.ChallengeAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = "/" });
	>                 }
	>             }
	>     
	>             [HttpGet]
	>             public async Task SignOut()
	>             {
	>                 string callbackUrl = Url.Action("SignOutCallback", "Account", values: null, protocol: "https");
	>     
	>                 if (HttpContext.User.Identity.IsAuthenticated)
	>                 {
	>                     string azureADUserObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
	>     
	>                     AuthenticationContext authenticationContext = new AuthenticationContext(Startup.AuthenticationAuthority, new NaiveSessionCache(azureADUserObjectID, HttpContext.Session));
	>                     authenticationContext.TokenCache.Clear();
	>     
	>                     await HttpContext.Authentication.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = callbackUrl });
	>                     await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme, new AuthenticationProperties { RedirectUri = callbackUrl });
	>                 }
	>             }
	>     
	>             public async Task SessionFinish()
	>             {
	>                 await HttpContext.Authentication.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
	>             }
	>     
	>             public ActionResult SignOutCallback()
	>             {
	>                 return RedirectToAction("Index", "Home");
	>             }
	>         }
	>     }

- Add new controller BaseController.cs to be the parent controller. Also change parent controller for HomeController and StudentController to be BaseController

	<img src="Images/29.PNG" width="80%">

	>     namespace FrameworkThree.Web.Controllers
	>     {
	>         public class BaseController : Controller
	>         {
	>             public BaseController()
	>             { }
	>     
	>             public async Task<AuthenticationResult> GetAuthenticationResult()
	>             {
	>                 string azureADUserObjectID = (User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier"))?.Value;
	>                 AuthenticationContext authenticationContext = new AuthenticationContext(Startup.AuthenticationAuthority, 
	>                                                                                     new NaiveSessionCache(azureADUserObjectID, HttpContext.Session));
	>     
	>                 ClientCredential clientCredential = new ClientCredential(Startup.ApplicationID, Startup.ApplicationKey);
	>                 AuthenticationResult authenticationResult = await authenticationContext.AcquireTokenSilentAsync(Startup.ServiceAppIDURI, 
	>                                                                                     clientCredential, 
	>                                                                                     new UserIdentifier(azureADUserObjectID, UserIdentifierType.UniqueId));
	>     
	>                 return authenticationResult;
	>             }
	>         }
	>     }

- Add [Authorize] attribute on StudentController

- Update ServiceInterface.cs file
	- Update baseServiceURL to use https URL
		>     private string baseServiceURL = "https://localhost:44344";

	-  Update GetDataAsync and PostDataToAPI methods to accept *AuthenticationResult authenticationResult* argument and set to *request.Headers.Authorization*

		>     request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authenticationResult.AccessToken);

- Update StudentController.cs file
	- Call GetAuthenticationResult method from BaseController and pass the authenticationResult to GetDataAsync and PostStudentData methods
	
		>     AuthenticationResult authenticationResult = await GetAuthenticationResult();
		>     response = await serviceInterface.GetDataAsync<List<StudentModel>>(authenticationResult, "student");
		>     --------
		>     AuthenticationResult authenticationResult = await GetAuthenticationResult();
		>     response = await serviceInterface.PostDataToAPI<StudentModel>(authenticationResult, "student", studentModel);

- Update _Layout file and add logic to show logged-in user name. Also give Signin/Signout buttons

	>     @if (User.Identity.IsAuthenticated)
	>     {
	>         <ul class="nav navbar-nav navbar-right">
	>             <li class="navbar-text">Hello, @User.Identity.Name</li>
	>             <li><a asp-controller="Account" asp-action="SignOut">Sign Out</a></li>
	>         </ul>
	>     }
	>     else
	>     {
	>         <ul class="nav navbar-nav navbar-right">
	>             <li><a asp-controller="Account" asp-action="Signin">Sign In</a></li>
	>         </ul>
	>     }

### Build and Run the code ###

- Make FrameworkTwo.Service and FrameworkTwo.Web as start up projects

- Set the launch URL for both projects FrameworkTwo.Service and FrameworkTwo.Web to use https

	<img src="Images/30.PNG" width="80%">
	<img src="Images/31.PNG" width="80%">

- Click on *Student Area* menu. You will be required to login, please login using your Azure AD credentials

- Test the *Get Students* and *Add Student* functionality

### References ###

- Azure-Samples -> github.com/Azure-Samples/active-directory-dotnet-webapp-webapi-openidconnect-aspnetcore (mainly NaiveSessionCache). For large applications its recommended to use database instead of session state)