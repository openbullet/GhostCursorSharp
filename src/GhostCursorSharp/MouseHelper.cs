using PuppeteerSharp;

namespace GhostCursorSharp;

/// <summary>
/// Installs a visual mouse helper into a Puppeteer page for debugging cursor movement.
/// </summary>
public static class MouseHelper
{
    private const string Script = """
        (() => {
          const attachListener = () => {
            if (!document.head || !document.body) {
              window.addEventListener('DOMContentLoaded', attachListener, { once: true });
              return;
            }

            window.__ghostCursorRemoveMouseHelper?.();

            const box = document.createElement('p-mouse-pointer');
            const styleElement = document.createElement('style');

            styleElement.innerHTML = `
              p-mouse-pointer {
                pointer-events: none;
                position: absolute;
                top: 0;
                z-index: 10000;
                left: 0;
                width: 20px;
                height: 20px;
                background: rgba(0,0,0,.4);
                border: 1px solid white;
                border-radius: 10px;
                box-sizing: border-box;
                margin: -10px 0 0 -10px;
                padding: 0;
                transition: background .2s, border-radius .2s, border-color .2s;
              }
              p-mouse-pointer.button-1 {
                transition: none;
                background: rgba(0,0,0,0.9);
              }
              p-mouse-pointer.button-2 {
                transition: none;
                border-color: rgba(0,0,255,0.9);
              }
              p-mouse-pointer.button-3 {
                transition: none;
                border-radius: 4px;
              }
              p-mouse-pointer.button-4 {
                transition: none;
                border-color: rgba(255,0,0,0.9);
              }
              p-mouse-pointer.button-5 {
                transition: none;
                border-color: rgba(0,255,0,0.9);
              }
              .p-mouse-pointer-hide {
                display: none;
              }
            `;

            document.head.appendChild(styleElement);
            document.body.appendChild(box);

            const updateButtons = (buttons) => {
              for (let i = 0; i < 5; i++) {
                box.classList.toggle(`button-${i}`, Boolean(buttons & (1 << i)));
              }
            };

            const onMouseMove = (event) => {
              box.style.left = `${event.pageX}px`;
              box.style.top = `${event.pageY}px`;
              box.classList.remove('p-mouse-pointer-hide');
              updateButtons(event.buttons);
            };

            const onMouseDown = (event) => {
              updateButtons(event.buttons);
              box.classList.add(`button-${event.which}`);
              box.classList.remove('p-mouse-pointer-hide');
            };

            const onMouseUp = (event) => {
              updateButtons(event.buttons);
              box.classList.remove(`button-${event.which}`);
              box.classList.remove('p-mouse-pointer-hide');
            };

            const onMouseLeave = (event) => {
              updateButtons(event.buttons);
              box.classList.add('p-mouse-pointer-hide');
            };

            const onMouseEnter = (event) => {
              updateButtons(event.buttons);
              box.classList.remove('p-mouse-pointer-hide');
            };

            document.addEventListener('mousemove', onMouseMove, true);
            document.addEventListener('mousedown', onMouseDown, true);
            document.addEventListener('mouseup', onMouseUp, true);
            document.addEventListener('mouseleave', onMouseLeave, true);
            document.addEventListener('mouseenter', onMouseEnter, true);

            window.__ghostCursorRemoveMouseHelper = () => {
              document.removeEventListener('mousemove', onMouseMove, true);
              document.removeEventListener('mousedown', onMouseDown, true);
              document.removeEventListener('mouseup', onMouseUp, true);
              document.removeEventListener('mouseleave', onMouseLeave, true);
              document.removeEventListener('mouseenter', onMouseEnter, true);
              box.remove();
              styleElement.remove();
              delete window.__ghostCursorRemoveMouseHelper;
            };
          };

          if (document.readyState !== 'loading') {
            attachListener();
          } else {
            window.addEventListener('DOMContentLoaded', attachListener, { once: true });
          }
        })();
        """;

    /// <summary>
    /// Installs a visual mouse helper into the page for the current document and future navigations.
    /// </summary>
    /// <param name="page">The page to decorate with the visual mouse helper.</param>
    /// <returns>An installation handle that can remove the helper later.</returns>
    public static async Task<MouseHelperInstallation> InstallAsync(IPage page)
    {
        var scriptIdentifier = await page.EvaluateExpressionOnNewDocumentAsync(Script);
        await page.EvaluateExpressionAsync(Script);

        return new MouseHelperInstallation(page, scriptIdentifier.Identifier);
    }
}
