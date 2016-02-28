﻿using UnityEngine;
using System.Collections;

public class MenuManager : MonoBehaviour
{
    static public MenuManager _instance;

    public LoginMenu loginMenu;
    public StartupMenu startupMenu;
    public SettingsMenu settingsMenu;
    public CreateAccountMenu accountMenu;

    public RectTransform loginGui;
    public RectTransform startupGui;
    public RectTransform settingsGui;
    public RectTransform accountgui;

    private RectTransform _currentPanel;

    void Start()
    {
        _instance = this;
        _currentPanel = loginGui;
    }

    public void ChangePanel(RectTransform newPanel)
    {
        if (_currentPanel != null)
        {
            _currentPanel.gameObject.SetActive(false);
        }

        if (newPanel != null)
        {
            newPanel.gameObject.SetActive(true);
        }

        _currentPanel = newPanel;
    }
}
