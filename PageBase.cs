using Microsoft.Playwright;
using Header = ConfirmedLayer.Tests.Pages.Common.Header;

namespace Playwright.Stable.Tests.Pages;

public abstract class PageBase(IPage page)
{
    protected readonly IPage Page = page;

    /// <summary>
    ///     Wait for the DOM to be completed and the Element getting Stable.This Method Atleast check Network Is Idle and Dom
    ///     Is stable.
    ///     Determine Locator Type:Check if the Locator is of Type XPATH, EvaluateAsync method handles both XPath and CSS
    ///     selector.
    ///     Stability Timeout: The stabilityTimeout ensures the DOM is stable for a specified period
    ///     Maximum Timeout: The maxTimeout ensures the promise resolves after a maximum period to prevent the method from
    ///     getting stuck indefinitely.
    /// </summary>
    protected async Task<ILocator> WaitForStableLocatorAsync(string selector, int stabilityTimeout = 500, int maxTimeout = 3000)
    {
        const int maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
            try
            {
                // Wait for the network to be idle
                await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                // Determine if the selector is XPath
                var isXPath = selector.StartsWith("//") || selector.StartsWith('/') || selector.StartsWith('(') ||
                              selector.StartsWith(".//");

                // Determine the locator based on the selector type
                var locator = isXPath ? Page.Locator($"xpath={selector}") : Page.Locator(selector);

                // Wait for the element to be present in the DOM
                await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
                await locator.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Attached });

                // Use MutationObserver to ensure no DOM changes
                await Page.EvaluateAsync(@"(params) => {
                return new Promise((resolve, reject) => {
                const targetNode = params.isXPath ? document.evaluate(params.selector, document, null, XPathResult.FIRST_ORDERED_NODE_TYPE, null).singleNodeValue : document.querySelector(params.selector);
                if (!targetNode) {
                    resolve();
                    return;
                }
                const observer = new MutationObserver((mutations, observer) => {
                    clearTimeout(window.domChangeTimeout);
                    window.domChangeTimeout = setTimeout(() => {
                        observer.disconnect();
                        resolve();
                    }, params.stabilityTimeout);
                });
                observer.observe(targetNode, { childList: true, subtree: true });

                // Maximum timeout to ensure the promise resolves
                const maxTimeoutId = setTimeout(() => {
                    observer.disconnect();
                    resolve();
                }, params.maxTimeout);

                // Clear the max timeout if the observer resolves first
                window.domChangeTimeout = setTimeout(() => {
                    clearTimeout(maxTimeoutId);
                    observer.disconnect();
                    resolve();
                }, params.stabilityTimeout);
                });
                }", new { selector, isXPath, stabilityTimeout, maxTimeout });

                return locator;
            }
            catch (PlaywrightException e) when (e.Message.Contains("Execution context was destroyed"))
            {
                retryCount++;
                if (retryCount >= maxRetries)
                    throw new Exception($"Failed to wait for stable Locator:{selector} after multiple retries.", e);

                // Wait for a short delay before retrying
                await Task.Delay(500);
            }

        // If all retries fail, throw an exception
        throw new Exception($"Failed to wait for stable Locator:{selector} after multiple retries.");
    }
}
    
