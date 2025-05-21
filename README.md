# 📄 PageBase Utility for Playwright in C# and .Net Core

## Overview

`PageBase` is an abstract base class designed to enhance Playwright test stability by ensuring that elements are interacted with only when the DOM is fully loaded and stable. This is particularly useful in scenarios where Playwright's speed causes flaky tests due to premature element interaction.

The core feature of this utility is the `WaitForStableLocatorAsync` method, which:

- Waits for the network to be idle.
- Ensures the element is attached and visible in the DOM.
- Uses a `MutationObserver` to confirm DOM stability before proceeding.

## Features

- ✅ DOM stability check using `MutationObserver`
- ✅ Supports both CSS and XPath selectors
- ✅ Retries on transient Playwright exceptions
- ✅ Configurable stability and timeout durations

---

## Installation

Include the `PageBase` class in your Playwright test project. It requires:

- Microsoft.Playwright
- C# 9.0 or later

---

## Usage

### Example: Creating a Page Object

```csharp
public class LoginPage : PageBase
{
    public LoginPage(IPage page) : base(page) { }

    public async Task<ILocator> GetUsernameInputAsync()
    {
        return await WaitForStableLocatorAsync("#username");
    }

    public async Task<ILocator> GetPasswordInputAsync()
    {
        return await WaitForStableLocatorAsync("#password");
    }

    public async Task<ILocator> GetLoginButtonAsync()
    {
        return await WaitForStableLocatorAsync("//button[text()='Login']");
    }
}
```

### Example: Using in a Test

```csharp
[Fact]
public async Task Login_ShouldSucceed_WhenCredentialsAreValid()
{
    var page = await Browser.NewPageAsync();
    var loginPage = new LoginPage(page);

    await page.GotoAsync("https://example.com/login");

    var usernameInput = await loginPage.GetUsernameInputAsync();
    await usernameInput.FillAsync("testuser");

    var passwordInput = await loginPage.GetPasswordInputAsync();
    await passwordInput.FillAsync("securepassword");

    var loginButton = await loginPage.GetLoginButtonAsync();
    await loginButton.ClickAsync();

    // Assert login success...
}
```

---

## Method: `WaitForStableLocatorAsync`

```csharp
protected async Task<ILocator> WaitForStableLocatorAsync(
    string selector,
    int stabilityTimeout = 500,
    int maxTimeout = 3000)
```

### Parameters

- `selector`: CSS or XPath selector for the target element.
- `stabilityTimeout`: Time (ms) the DOM must remain unchanged before proceeding.
- `maxTimeout`: Maximum time (ms) to wait before giving up.

### Returns

- A stable `ILocator` object ready for interaction.

### Throws

- `Exception` if the element is not stable after multiple retries.

---

## Notes

- This utility is ideal for dynamic web applications where DOM mutations are frequent.
- It helps reduce test flakiness by ensuring elements are stable before interaction.

---
