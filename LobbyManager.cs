using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager instance;
    [Header("UI References")]
    [SerializeField]
    private GameObject profileUI;
    [SerializeField]
    private GameObject changePfpUI;
    [SerializeField]
    private GameObject changeEmailUI;
    [SerializeField]
    private GameObject changePasswordUI;
    [SerializeField]
    private GameObject reverifyUI;
    [SerializeField]
    private GameObject resetPasswordConfirmUI;
    [SerializeField]
    private GameObject actionSuccessPanelUI;
    [SerializeField]
    private GameObject deleteUserConfirmUI;
    [Space(5f)]

    [Header("Basic Info References")]
    [SerializeField]
    private TMP_Text usernameText;
    [SerializeField]
    private TMP_Text emailText;
    [SerializeField]
    private string token;
    [Space(5f)]

    [Header("Profile Picture References")]
    [SerializeField]
    private Image profilePicture;
    [SerializeField]
    private TMP_InputField profilePictureLink;
    [SerializeField]
    private TMP_Text outputText;
    [Space(5f)]

    [Header("Change Email References")]
    [SerializeField]
    private TMP_InputField changeEmailEmailInputField;
    [Space(5f)]

    [Header("Change Password References")]
    [SerializeField]
    private TMP_InputField changePasswordInputField;
    [SerializeField]
    private TMP_InputField changePasswordConfirmInputField;
    [Space(5f)]


    [Header("Reverify References")]
    [SerializeField]
    private TMP_InputField reverifyEmailInputField;
    [SerializeField]
    private TMP_InputField reverifyPasswordInputField;
    [Space(5f)]

    [Header("Action Success Panel References")]
    [SerializeField]
    private TMP_Text actionSuccessText;


    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (FirebaseManager.instance.user != null)
        {
            LoadProfile();
        }
    }

    private void LoadProfile()
    {
        if (FirebaseManager.instance.user != null)
        {
            //get variables:
            System.Uri photoUrl = FirebaseManager.instance.user.PhotoUrl;
            string name = FirebaseManager.instance.user.DisplayName;
            string email = FirebaseManager.instance.user.Email;

            //set UI
            StartCoroutine(LoadImage(photoUrl.ToString()));
            usernameText.text = name;
            emailText.text = email;

        }
    }

    private IEnumerator LoadImage(string _photoUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(_photoUrl);
        yield return request.SendWebRequest();
        if (request.error != null)
        {
            string output = "Unknown Error! Try Again!";

            if (request.isHttpError)
            {
                output = "Image Type not supported! Please try another image.";
            }
            Output(output);


        }
        else
        {
            Texture2D image = ((DownloadHandlerTexture)request.downloadHandler).texture;
            profilePicture.sprite = Sprite.Create(image, new Rect(0, 0, image.width, image.height), Vector2.zero);
        }
    }

    public void Output(string _output)
    {
        outputText.text = _output;
    }

    public void ClearUI()
    {
        outputText.text = "";
        profileUI.SetActive(false);
        changePfpUI.SetActive(false);
        actionSuccessPanelUI.SetActive(false);
    }

    public void ProfileUI()
    {
        ClearUI();
        profileUI.SetActive(true);
        LoadProfile();
    }

    public void ChangePfpUI()
    {
        ClearUI();
        
        changePfpUI.SetActive(true);
    }

    public void ChangePfpSuccess()
    {
        ClearUI();
        actionSuccessPanelUI.SetActive(true);
        actionSuccessText.text = "Profile Picture Changed Successfully!";
    }

    public void SubmitProfileImageButton()
    {
        FirebaseManager.instance.UpdateProfilePicture(profilePictureLink.text);
    }
    //below function is called when sign out button is clicked:
    public void LogoutButton()
    {
        FirebaseManager.instance.auth.SignOut(); //this line would log you out from the game
        Debug.Log("Signed out");                 //will print signed out in the console
        GameManager.instance.ChangeScene(0);     //will change the scene back to login screen
    }
    //below is the function for start game button
    public void StartGameButton()
    {

        Debug.Log("Game started!");                 //will print Game started in the console
        GameManager.instance.ChangeScene(2);     //will change the scene to the to Game or "Sample Scene"
    }


    //below is the function which works on click of "Sounds good button!"
    public void SoundsGoodButton()
    {

        Debug.Log("Profile picture changed!");                 //will print Game started in the console
        GameManager.instance.ChangeScene(1);     //will change the scene back to lobby
    }


    //changing to password UI below:
    public void ChangePasswordUI()
    {
        ClearUI();
        changePasswordUI.SetActive(true);
    }

    //updating password or changing password below


    public  void changePasswordSubmitButton()
    {
        string emailAddress = FirebaseManager.instance.user.Email;

        FirebaseManager.instance.auth.SendPasswordResetEmailAsync(emailAddress).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                    return;
                }


                Debug.Log("Password reset email sent successfully.");
            
        });

        
}


    //changing to reverify account UI below:
    public void ReverifyUI()
    {
        ClearUI();
        reverifyUI.SetActive(true);
    }

    //reverifying account below


    public void reverifySubmitButton()
    {
        string emailAddress = FirebaseManager.instance.user.Email;

        FirebaseManager.instance.user.SendEmailVerificationAsync().ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("SendEmailVerificationAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("SendEmailVerificationAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("Email sent successfully.");
            });
        



    }

    //below is the back button for reverify UI:

    public void reverifyBack()
    {
        
        Debug.Log("Successfully Reverified!");                 
        GameManager.instance.ChangeScene(1);     //will change the scene back to lobby scene
    }


    //changing to delete user UI below:
    public void DeleteUserConfirmUI()
    {
        ClearUI();
        deleteUserConfirmUI.SetActive(true);
    }

    //deletling account below


    public void deleteConfirmSubmitButton()
    {
        


        FirebaseManager.instance.user.DeleteAsync().ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("DeleteAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("DeleteAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User deleted successfully.");
            
            });
        



    }

    //below is the back button for delete UI:

    public void changeToLobby()
    {


        //GameManager.instance.ChangeScene(1);     //will change the scene back to lobby scene
        SceneManager.LoadScene(1);
        
    }




}














