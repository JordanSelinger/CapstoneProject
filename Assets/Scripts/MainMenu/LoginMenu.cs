﻿using UnityEngine;
using UnityEngine.UI;
using System;

public class LoginMenu : MonoBehaviour
{
    public Button loginButton;
    public Button quitButton;
    public Button createAccountButton;
	public Button guestButton;

    public Text errorText;

    public InputField usernameInputField;
    public InputField passwordInputField;

    public void OnEnable()
    {
        loginButton.onClick.RemoveAllListeners();
        loginButton.onClick.AddListener(LoginButton_OnClick);

        quitButton.onClick.RemoveAllListeners();
        quitButton.onClick.AddListener(QuitButton_OnClick);

        createAccountButton.onClick.RemoveAllListeners();
        createAccountButton.onClick.AddListener(CreateAccount_OnClick);

		guestButton.onClick.RemoveAllListeners ();
		guestButton.onClick.AddListener (GuestButton_OnClick);

        usernameInputField.ActivateInputField();
        passwordInputField.ActivateInputField();

        errorText.text = "";
		passwordInputField.text = "";

        if (LoginInformation.userName != null && LoginInformation.userName != "" && LoginInformation.loggedIn)
            usernameInputField.text = LoginInformation.userName;
    }

    public void LoginButton_OnClick()
    {
        var db = DBConnection.GetInstance();

        var response = db.Login(new LoginMessage { userName = usernameInputField.text, password = passwordInputField.text });

        if (!response.isSuccessful)
            errorText.text = response.errorMessage;
        else
        {
            LoginInformation.userName = usernameInputField.text;
            LoginInformation.guid = new System.Guid(response.userId);
            LoginInformation.loggedIn = true;
            MenuManager.instance.ChangePanel(MenuManager.instance.startupGui);
        }
    }

	public void GuestButton_OnClick()
	{
		LoginInformation.userName = "Guest";
		LoginInformation.guid = Guid.Empty;
		LoginInformation.loggedIn = true;

		MenuManager.instance.ChangePanel (MenuManager.instance.startupGui);
	}

    public void CreateAccount_OnClick()
    {
        MenuManager.instance.ChangePanel(MenuManager.instance.accountgui);
    }

    public void QuitButton_OnClick()
    {
        Application.Quit();
    }
}
