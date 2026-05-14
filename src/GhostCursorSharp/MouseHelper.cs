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
              p-mouse-pointer-pulse {
                pointer-events: none;
                position: absolute;
                z-index: 9999;
                width: 18px;
                height: 18px;
                margin: -9px 0 0 -9px;
                border-radius: 999px;
                border: 2px solid rgba(255,255,255,0.95);
                box-shadow: 0 0 0 1px rgba(0,0,0,0.16);
                animation: p-mouse-pointer-pulse .42s ease-out forwards;
              }
              p-mouse-pointer-pulse.mouse-down {
                border-color: rgba(255, 174, 66, 0.95);
              }
              p-mouse-pointer-pulse.mouse-up {
                border-color: rgba(99, 179, 237, 0.95);
              }
              p-mouse-pointer-pulse.mouse-click {
                border-color: rgba(72, 187, 120, 0.98);
              }
              @keyframes p-mouse-pointer-pulse {
                0% {
                  opacity: 0.9;
                  transform: scale(0.35);
                }
                100% {
                  opacity: 0;
                  transform: scale(2.7);
                }
              }
            `;

            document.head.appendChild(styleElement);
            document.body.appendChild(box);

            const updateButtons = (buttons) => {
              for (let i = 0; i < 5; i++) {
                box.classList.toggle(`button-${i}`, Boolean(buttons & (1 << i)));
              }
            };

            const emitPulse = (x, y, className) => {
              const pulse = document.createElement('p-mouse-pointer-pulse');
              pulse.classList.add(className);
              pulse.style.left = `${x}px`;
              pulse.style.top = `${y}px`;
              document.body.appendChild(pulse);
              pulse.addEventListener('animationend', () => pulse.remove(), { once: true });
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
              emitPulse(event.pageX, event.pageY, 'mouse-down');
            };

            const onMouseUp = (event) => {
              updateButtons(event.buttons);
              box.classList.remove(`button-${event.which}`);
              box.classList.remove('p-mouse-pointer-hide');
              emitPulse(event.pageX, event.pageY, 'mouse-up');
            };

            const onClick = (event) => {
              box.classList.remove('p-mouse-pointer-hide');
              emitPulse(event.pageX, event.pageY, 'mouse-click');
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
            document.addEventListener('click', onClick, true);
            document.addEventListener('mouseleave', onMouseLeave, true);
            document.addEventListener('mouseenter', onMouseEnter, true);

            window.__ghostCursorRemoveMouseHelper = () => {
              document.removeEventListener('mousemove', onMouseMove, true);
              document.removeEventListener('mousedown', onMouseDown, true);
              document.removeEventListener('mouseup', onMouseUp, true);
              document.removeEventListener('click', onClick, true);
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
