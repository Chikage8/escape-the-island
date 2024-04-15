using System.Collections;

using UnityEngine;
using Firebase;
using Firebase.Auth;
using TMPro;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance;

    [Header("Firebase")]
    public FirebaseAuth auth;
    public FirebaseUser user;
    [Space(5f)]
    [Header("Login References")]
    [SerializeField]
    private TMP_InputField loginEmail;
    [SerializeField]
    private TMP_InputField loginPassword;
    [SerializeField]
    private TMP_Text loginOutputText;
    [Space(5f)]
    [Header("Register References")]
[SerializeField]
private TMP_InputField registerUsername;
[SerializeField]
private TMP_InputField registerEmail;
[SerializeField]
private TMP_InputField registerPassword;

[SerializeField]
private TMP_InputField registerConfirmPassword;
[SerializeField]
private TMP_Text registerOutputText;






public void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(instance.gameObject);
            instance = this;
        }
   
    }
    //below is the code for auto login:

    private void Start()
    {
        StartCoroutine(CheckAndFixDependancies());
    }

    private IEnumerator CheckAndFixDependancies()
    {
        var checkAndFixDependanciesTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(predicate: () => checkAndFixDependanciesTask.IsCompleted);
        var dependancyResult = checkAndFixDependanciesTask.Result;
        if (dependancyResult == DependencyStatus.Available)
        {
            InitializeFirebase();
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependancyResult}");
        }
    }
       private void InitializeFirebase()
        {
         auth = FirebaseAuth.DefaultInstance;
        StartCoroutine(CheckAutoLogin());  //checking Auto Login after the Autorization is set
            auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
        }
    private IEnumerator CheckAutoLogin()
    {
        yield return new WaitForEndOfFrame();
        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();
            yield return new WaitUntil(predicate: () => reloadUserTask.IsCompleted);
            AutoLogin();   //if the user exists so it will call AutoLogin and the user would be logged in automatically
        }
        else     //else it will continue back to the login screen
        {
            AuthUIManager.instance.LoginScreen();

        }

    }

    private void AutoLogin()  //here again check is there is a user, if there is a user so he will be sent to the lobby screen 
    {
        if (user != null) //if user exists
        {
            if (user.IsEmailVerified)  //If email verified so move to next screen!
            {
                GameManager.instance.ChangeScene(1);  //changing scene to lobby
            }
            else  //if not verified so send an verification email
            {
                StartCoroutine(sendVerificationEmail()); //otherwise send an email
            }
        }
        else
        {
            AuthUIManager.instance.LoginScreen(); //calling login screen back
        }
    }




 

private void AuthStateChanged(object sender, System.EventArgs eventArgs)
{
    if(auth.CurrentUser != user)
    {
        bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;
        if(!signedIn && user != null)
        {
            Debug.Log("Signed Out");
        }
        user = auth.CurrentUser;
        if (signedIn)
        {
            Debug.Log($"Signed In: {user.DisplayName}");
        }
    }
}
public void ClearOutputs()  //it will clear the outputs which were previously displayed due to errors. for example error displayed due to weak password
{
    loginOutputText.text = "";
    registerOutputText.text = "";
}

public void LoginButton()
{
        StartCoroutine(LoginLogic(loginEmail.text, loginPassword.text));
}
public void RegisterButton()
{
        StartCoroutine(RegisterLogic(registerUsername.text, registerEmail.text, registerPassword.text, registerConfirmPassword.text));

    }

private IEnumerator LoginLogic(string _email, string _password)
{
    Credential credential = EmailAuthProvider.GetCredential(_email, _password);

    var loginTask = auth.SignInWithCredentialAsync(credential);
    yield return new WaitUntil(predicate: () => loginTask.IsCompleted);

    if(loginTask.Exception != null)
    {
        FirebaseException firebaseException = (FirebaseException) loginTask.Exception.GetBaseException();
        AuthError error = (AuthError)firebaseException.ErrorCode;
        string output = "Unknown Error, Please Try Again!";

        switch (error)
        {
            case AuthError.MissingEmail:
                output = "Please Enter Your Email";
                break;
            case AuthError.MissingPassword:
                output = "Please Enter Your Password";
                break;
            case AuthError.InvalidEmail:
                output = "Invalid Email";
                break;
            case AuthError.WrongPassword:
                output = "Incorrect Password";
                break;
            case AuthError.UserNotFound:
                output = "Account Doesn't Exist";
                break;

        }
        loginOutputText.text = output;

    }
    else
    {
        if (user.IsEmailVerified)
        {
            yield return new WaitForSeconds(1f);
            GameManager.instance.ChangeScene(1);  //change to lobby screen
        }
        else
        {
                //send verfivation email
                StartCoroutine(sendVerificationEmail());
        }
    }
       

}

    private IEnumerator RegisterLogic(string _username, string _email, string _password, string _confirmPassword)
    {
        if (_username == "")
        {
            registerOutputText.text = "Please enter a username";
        }
        else if (_password != _confirmPassword)
        {
            registerOutputText.text = "Passwords don't match!!";
        }

        else
        {
            var registerTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            yield return new WaitUntil(predicate: () => registerTask.IsCompleted);
            if (registerTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException) registerTask.Exception.GetBaseException();
                AuthError error = (AuthError)firebaseException.ErrorCode;
                string output = "Unknown Error, Please Try Again!";

                switch (error)
                {
                    case AuthError.InvalidEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        output = "Email Already In Use!";
                        break;
                    case AuthError.WeakPassword:
                        output = "Weak Password";
                        break;
                    case AuthError.MissingEmail:
                        output = "Please enter your Email!";
                        break;
                    case AuthError.MissingPassword:
                        output = "Please enter your password";
                        break;

                }
                registerOutputText.text = output;
            }
            else
            {
                UserProfile profile = new UserProfile
                {
                    DisplayName = _username,
                    PhotoUrl=new System.Uri("https://thumbs.dreamstime.com/z/sun-rays-mountain-landscape-5721010.jpg"),
                };
        
        var defaultUserTask = user.UpdateUserProfileAsync(profile);
        yield return new WaitUntil(predicate: () => defaultUserTask.IsCompleted);

        if (defaultUserTask.Exception != null)
        {
            user.DeleteAsync();
            FirebaseException firebaseException = (FirebaseException)defaultUserTask.Exception.GetBaseException();
            AuthError error = (AuthError)firebaseException.ErrorCode;
            string output = "Unknown Error, Please Try Again!";

            switch (error)
            {
                case AuthError.Cancelled:
                    output = "Update User cancelled";
                    break;
                case AuthError.SessionExpired:
                    output = "Session Expired";
                    break;
                

            }
            registerOutputText.text = output;

        }
        else
        {
            Debug.Log($"Firebase User created successfully: {user.DisplayName}({user.UserId})");
                    StartCoroutine(sendVerificationEmail()); //send verification email
        }
        }
    }
}

    private IEnumerator sendVerificationEmail()
    {
        if (user != null)
        {
            var emailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(predicate: ()=> emailTask.IsCompleted);

            if (emailTask.Exception != null)
            {
                FirebaseException firebaseException = (FirebaseException) emailTask.Exception.GetBaseException();
                AuthError error = (AuthError) firebaseException.ErrorCode;

                string output = "unknown error, Try Again!";

                switch (error)
                {
                    case AuthError.Cancelled:
                        output = "Verification Task was Cancelled";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        output = "Invalid Email";
                        break;
                    case AuthError.TooManyRequests:
                        output = "Too Many Requests";
                        break;
                }

                AuthUIManager.instance.AwaitVerification(false, user.Email, output);
            }
            else
            {
                AuthUIManager.instance.AwaitVerification(true, user.Email, null);
                Debug.Log("Email Sent Successfully");

            }
        }
    }
    public void UpdateProfilePicture(string _newPfpURL)
    {
        StartCoroutine(UpdateProfilePictureLogic(_newPfpURL));
    }
    private IEnumerator UpdateProfilePictureLogic(string _newPfpURL)
    {
        if (user != null)
        {
            UserProfile profile = new UserProfile();
            try
            {
                UserProfile _profile = new UserProfile
                {
                    PhotoUrl = new System.Uri(_newPfpURL),
                };
                profile = _profile;
            }
            catch
            {
                LobbyManager.instance.Output("Error Fetching Image, Make sure your link is valid!");
                yield break;
            }
            var pfpTask = user.UpdateUserProfileAsync(profile);
            yield return new WaitUntil(predicate: () => pfpTask.IsCompleted);

            if(pfpTask.Exception != null)
            {
                Debug.LogError($"Updating Profile Picture was unsuccessful: {pfpTask.Exception}");
            }
            else
            {
                LobbyManager.instance.ChangePfpSuccess();
                Debug.Log("Profile Image updated successfully");
            }
        }
    }




}

   







