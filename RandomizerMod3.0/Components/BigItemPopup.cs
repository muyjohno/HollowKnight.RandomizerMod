﻿using System.Collections;
using Modding;
using RandomizerMod.Actions;
using RandomizerMod.Randomization;
using SereCore;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

namespace RandomizerMod.Components
{
    internal class BigItemPopup : MonoBehaviour
    {
        private static readonly Sprite BlackPixel = CanvasUtil.NullSprite(new byte[] {0x00, 0x00, 0x00, 0xAA});
        private static readonly Sprite[] Frames;
        private string _buttonText;
        private string _descOneText;
        private string _descTwoText;
        private string _fsmEvent;
        private GameObject _fsmObj;

        private Sprite _imagePrompt;
        private string _nameText;

        private bool _showInstantly;
        private string _takeText;

        static BigItemPopup()
        {
            Frames = new[]
            {
                Sprites.GetSprite("Anim.BigItemFleur.0"),
                Sprites.GetSprite("Anim.BigItemFleur.1"),
                Sprites.GetSprite("Anim.BigItemFleur.2"),
                Sprites.GetSprite("Anim.BigItemFleur.3"),
                Sprites.GetSprite("Anim.BigItemFleur.4"),
                Sprites.GetSprite("Anim.BigItemFleur.5"),
                Sprites.GetSprite("Anim.BigItemFleur.6"),
                Sprites.GetSprite("Anim.BigItemFleur.7"),
                Sprites.GetSprite("Anim.BigItemFleur.8")
            };
        }

        public static bool AdditiveMaxedOut(BigItemDef[] items)
        {
            int count = RandomizerMod.Instance.Settings.GetAdditiveCount(items[0].Name);
            return count >= items.Length;
        }

        public static GameObject ShowAdditive(BigItemDef[] items, GameObject fsmObj = null, string eventName = null)
        {
            int count = RandomizerMod.Instance.Settings.GetAdditiveCount(items[0].Name);

            BigItemDef shownItem = items[count];
            // Extra code so that when we get L/R shade cloak after having the other MWC, we just show the popup for shade cloak
            // We *only* want to switch to showing Shade Cloak when we have exactly one dash in each direction; otherwise
            // we'll just show Left and Right Shade Cloaks as usual
            // - Deactivated because I felt that destroying the information about which shade cloak it is is more
            // annoying than showing an incorrect dash direction.

            /*
            if (items[0].Name == "Left_Mothwing_Cloak" || items[0].Name == "Right_Mothwing_Cloak"
                || items[0].Name == "Left_Shade_Cloak" || items[0].Name == "Right_Shade_Cloak")
            {
                if (RandomizerMod.Instance.Settings.GetAdditiveCount("Left_Mothwing_Cloak") == 1
                    && RandomizerMod.Instance.Settings.GetAdditiveCount("Right_Mothwing_Cloak") == 1)
                {
                    ReqDef shadeCloak = LogicManager.GetItemDef("Shade_Cloak");
                    shownItem = new BigItemDef
                    {
                        Name = shownItem.Name,
                        BoolName = shadeCloak.boolName,
                        SpriteKey = shadeCloak.bigSpriteKey,
                        TakeKey = shadeCloak.takeKey,
                        NameKey = shadeCloak.nameKey,
                        ButtonKey = shadeCloak.buttonKey,
                        DescOneKey = shadeCloak.descOneKey,
                        DescTwoKey = shadeCloak.descTwoKey
                    };
                }
            }
            */

            return Show(shownItem, fsmObj, eventName);
        }

        public static GameObject Show(BigItemDef item, GameObject fsmObj = null, string eventName = null)
        {
            //Ref.PD.SetBool(item.BoolName, true);
            return Show(item.SpriteKey, item.TakeKey, item.NameKey, item.ButtonKey, item.DescOneKey, item.DescTwoKey,
                fsmObj, eventName);
        }

        public static GameObject Show(string spriteKey, string takeKey, string nameKey, string buttonKey,
            string descOneKey, string descTwoKey, GameObject fsmObj = null, string eventName = null)
        {
            // Create base canvas
            GameObject canvas = CanvasUtil.CreateCanvas(RenderMode.ScreenSpaceOverlay, new Vector2(1920, 1080));

            // Add popup component, set values
            BigItemPopup popup = canvas.AddComponent<BigItemPopup>();
            popup._imagePrompt = Sprites.GetSprite(spriteKey);
            popup._takeText = Language.Language.Get(takeKey, "Prompts").Replace("<br>", " ");
            popup._nameText = Language.Language.Get(nameKey, "UI").Replace("<br>", " ");
            popup._buttonText = Language.Language.Get(buttonKey, "Prompts").Replace("<br>", " ");
            popup._descOneText = Language.Language.Get(descOneKey, "Prompts").Replace("<br>", " ");
            popup._descTwoText = Language.Language.Get(descTwoKey, "Prompts").Replace("<br>", " ");
            popup._fsmObj = fsmObj;
            popup._fsmEvent = eventName;

            return canvas;
        }

        public void Start()
        {
            SereCore.Ref.GM.SaveGame(SereCore.Ref.GM.profileID, x => { });
            StartCoroutine(ShowPopup());
        }

        private IEnumerator ShowPopup()
        {
            // Check for skipping popup
            Coroutine skipCoroutine = StartCoroutine(LookForShowInstantly());

            // Begin dimming the scene
            GameObject dimmer = CanvasUtil.CreateImagePanel(gameObject, BlackPixel,
                new CanvasUtil.RectData(Vector2.zero, Vector2.zero, Vector2.zero, Vector2.one));
            dimmer.GetComponent<Image>().preserveAspect = false;
            CanvasGroup dimmerCG = dimmer.AddComponent<CanvasGroup>();

            dimmerCG.blocksRaycasts = false;
            dimmerCG.interactable = false;
            dimmerCG.alpha = 0;

            StartCoroutine(FadeInCanvasGroup(dimmerCG));

            yield return WaitForSeconds(0.1f);

            // Aim for 400 high prompt image
            float scaler = _imagePrompt.texture.height / 400f;
            Vector2 size = new Vector2(_imagePrompt.texture.width / scaler, _imagePrompt.texture.height / scaler);

            // Begin fading in the top bits of the popup
            GameObject topImage = CanvasUtil.CreateImagePanel(gameObject, _imagePrompt,
                new CanvasUtil.RectData(size, Vector2.zero, new Vector2(0.5f, 0.75f), new Vector2(0.5f, 0.8f)));
            GameObject topTextOne = CanvasUtil.CreateTextPanel(gameObject, _takeText, 34, TextAnchor.MiddleCenter,
                new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.55f),
                    new Vector2(0.5f, 0.55f)), Fonts.Get("Perpetua"));
            GameObject topTextTwo = CanvasUtil.CreateTextPanel(gameObject, _nameText, 76, TextAnchor.MiddleCenter,
                new CanvasUtil.RectData(new Vector2(1920, 300), Vector2.zero, new Vector2(0.5f, 0.49f),
                    new Vector2(0.5f, 0.49f)));

            CanvasGroup topImageCG = topImage.AddComponent<CanvasGroup>();
            CanvasGroup topTextOneCG = topTextOne.AddComponent<CanvasGroup>();
            CanvasGroup topTextTwoCG = topTextTwo.AddComponent<CanvasGroup>();

            topImageCG.blocksRaycasts = false;
            topImageCG.interactable = false;
            topImageCG.alpha = 0;

            topTextOneCG.blocksRaycasts = false;
            topTextOneCG.interactable = false;
            topTextOneCG.alpha = 0;

            topTextTwoCG.blocksRaycasts = false;
            topTextTwoCG.interactable = false;
            topTextTwoCG.alpha = 0;

            StartCoroutine(FadeInCanvasGroup(topImageCG));
            StartCoroutine(FadeInCanvasGroup(topTextOneCG));
            yield return StartCoroutine(FadeInCanvasGroup(topTextTwoCG));

            // Animate the middle fleur
            GameObject fleur = CanvasUtil.CreateImagePanel(gameObject, Frames[0],
                new CanvasUtil.RectData(new Vector2(Frames[0].texture.width / 1.6f, Frames[0].texture.height / 1.6f),
                    Vector2.zero, new Vector2(0.5f, 0.4125f), new Vector2(0.5f, 0.4125f)));
            yield return StartCoroutine(AnimateFleur(fleur, 12));
            yield return WaitForSeconds(0.25f);

            // Fade in the remaining text
            GameObject botTextOne = CanvasUtil.CreateTextPanel(gameObject, _buttonText, 34, TextAnchor.MiddleCenter,
                new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.335f),
                    new Vector2(0.5f, 0.335f)), Fonts.Get("Perpetua"));
            GameObject botTextTwo = CanvasUtil.CreateTextPanel(gameObject, _descOneText, 34, TextAnchor.MiddleCenter,
                new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.26f),
                    new Vector2(0.5f, 0.26f)), Fonts.Get("Perpetua"));
            GameObject botTextThree = CanvasUtil.CreateTextPanel(gameObject, _descTwoText, 34, TextAnchor.MiddleCenter,
                new CanvasUtil.RectData(new Vector2(1920, 100), Vector2.zero, new Vector2(0.5f, 0.205f),
                    new Vector2(0.5f, 0.205f)), Fonts.Get("Perpetua"));

            CanvasGroup botTextOneCG = botTextOne.AddComponent<CanvasGroup>();
            CanvasGroup botTextTwoCG = botTextTwo.AddComponent<CanvasGroup>();
            CanvasGroup botTextThreeCG = botTextThree.AddComponent<CanvasGroup>();

            botTextOneCG.blocksRaycasts = false;
            botTextOneCG.interactable = false;
            botTextOneCG.alpha = 0;

            botTextTwoCG.blocksRaycasts = false;
            botTextTwoCG.interactable = false;
            botTextTwoCG.alpha = 0;

            botTextThreeCG.blocksRaycasts = false;
            botTextThreeCG.interactable = false;
            botTextThreeCG.alpha = 0;

            yield return StartCoroutine(FadeInCanvasGroup(botTextOneCG));
            StartCoroutine(FadeInCanvasGroup(botTextTwoCG));
            yield return StartCoroutine(FadeInCanvasGroup(botTextThreeCG));
            yield return WaitForSeconds(1.5f);

            // Can I offer you an egg in this trying time?
            GameObject egg = CanvasUtil.CreateImagePanel(gameObject, Sprites.GetSprite("UI.egg"),
                new CanvasUtil.RectData(
                    new Vector2(Sprites.GetSprite("UI.egg").texture.width / 1.65f,
                        Sprites.GetSprite("UI.egg").texture.height / 1.65f), Vector2.zero,
                    new Vector2(0.5f, 0.1075f), new Vector2(0.5f, 0.1075f)));
            CanvasGroup eggCG = egg.AddComponent<CanvasGroup>();

            eggCG.blocksRaycasts = false;
            eggCG.interactable = false;
            eggCG.alpha = 0;

            // Should wait for one fade in, don't want to poll input immediately
            yield return FadeInCanvasGroup(eggCG);

            // Stop doing things instantly before polling input
            if (!_showInstantly)
            {
                StopCoroutine(skipCoroutine);
            }

            _showInstantly = false;

            // Save the coroutine to stop it later
            Coroutine coroutine = StartCoroutine(BlinkCanvasGroup(eggCG));

            // Wait for the user to cancel the menu
            while (true)
            {
                HeroActions actions = SereCore.Ref.Input.inputActions;
                if (actions.jump.WasPressed || actions.attack.WasPressed || actions.menuCancel.WasPressed)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            // Fade out the full popup
            yield return FadeOutCanvasGroup(gameObject.GetComponent<CanvasGroup>());

            // Small delay before hero control
            yield return WaitForSeconds(0.75f);

            // Optionally send FSM event after finishing
            if (_fsmObj != null && _fsmEvent != null)
            {
                FSMUtility.SendEventToGameObject(_fsmObj, _fsmEvent);
            }

            // Stop the egg routine and destroy everything
            StopCoroutine(coroutine);
            Destroy(gameObject);
        }

        private IEnumerator AnimateFleur(GameObject fleur, float fps)
        {
            Image img = fleur.GetComponent<Image>();
            int spriteNum = 0;

            while (spriteNum < Frames.Length)
            {
                img.sprite = Frames[spriteNum];
                spriteNum++;
                yield return WaitForSeconds(1 / fps);
            }
        }

        // ReSharper disable once IteratorNeverReturns
        private IEnumerator BlinkCanvasGroup(CanvasGroup cg)
        {
            while (true)
            {
                yield return FadeOutCanvasGroup(cg);
                yield return FadeInCanvasGroup(cg);
            }
        }

        private IEnumerator WaitForSeconds(float seconds)
        {
            float timePassed = 0f;
            while (timePassed < seconds && !_showInstantly)
            {
                timePassed += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
        }

        private IEnumerator LookForShowInstantly()
        {
            while (true)
            {
                HeroActions actions = SereCore.Ref.Input.inputActions;
                if (actions.jump.WasPressed || actions.attack.WasPressed || actions.menuCancel.WasPressed)
                {
                    _showInstantly = true;
                    break;
                }

                yield return new WaitForEndOfFrame();
            }
        }

        // Below functions ripped from CanvasUtil in order to change the speed
        private IEnumerator FadeInCanvasGroup(CanvasGroup cg)
        {
            float loopFailsafe = 0f;
            cg.alpha = 0f;
            cg.gameObject.SetActive(true);
            while (cg.alpha < 1f && !_showInstantly)
            {
                cg.alpha += Time.deltaTime * 2f;
                loopFailsafe += Time.deltaTime;
                if (cg.alpha >= 0.95f)
                {
                    cg.alpha = 1f;
                    break;
                }

                if (loopFailsafe >= 2f)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            cg.alpha = 1f;
            cg.interactable = true;
            cg.gameObject.SetActive(true);
            yield return new WaitForEndOfFrame();
        }

        // Identical to CanvasUtil version except it doesn't randomly set the canvas object inactive at the end
        private IEnumerator FadeOutCanvasGroup(CanvasGroup cg)
        {
            float loopFailsafe = 0f;
            cg.interactable = false;
            while (cg.alpha > 0.05f && !_showInstantly)
            {
                cg.alpha -= Time.deltaTime * 2f;
                loopFailsafe += Time.deltaTime;
                if (cg.alpha <= 0.05f)
                {
                    break;
                }

                if (loopFailsafe >= 2f)
                {
                    break;
                }

                yield return new WaitForEndOfFrame();
            }

            cg.alpha = 0f;
            yield return new WaitForEndOfFrame();
        }
    }
}
