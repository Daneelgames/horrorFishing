using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryViewerController : MonoBehaviour
{
    public enum InventoryType { Inventory, Memento, Notes, Quests, QuestItems, Whispers, Leaderboard}

    public InventoryType type = InventoryType.Inventory;
    
    public List<Image> mementoImagesSlots;
    public Image mementoImageSlot;
    public TextMeshProUGUI selectedMementoInfo;
    public Image selectedMementoMark;
    public Image mementoHolderImage;
    private ItemsList il;
    private PlayerSkillsController psc;
    public Button leaderboardStartButton;
    
    public TextMeshProUGUI whispersLevelInfo;
    public TextMeshProUGUI whispersOfTheGut;
    public TextMeshProUGUI whispersOfTheGut2;

    private GameManager gm;
    private QuestManager qm;
    private UiManager ui;
    
    public AudioSource panelSwitchSourse;
    public AudioSource switchItemSourse;
    
    
    public List<GameObject> notesLocals = new List<GameObject>();
    
    public List<int> availableTools = new List<int>();

    void Start()
    {
        il = ItemsList.instance;
        psc = PlayerSkillsController.instance;
        gm = GameManager.instance;
        ui = UiManager.instance;
    }
    
    public void Init()
    {
        il = ItemsList.instance;
        psc = PlayerSkillsController.instance;
        gm = GameManager.instance;
        ui = UiManager.instance;

        if (panelSwitchSourse)
        {
            panelSwitchSourse.pitch = Random.Range(0.75f, 1.25f);
            panelSwitchSourse.Play();   
        }
        
        //Close();
        
        if (type != InventoryType.Quests)
        {
            SelectMemento(-1);
        }

        if (type == InventoryType.Inventory)
        {
            // if has weapons
            if (il.activeWeapon != null && il.activeWeapon != WeaponPickUp.Weapon.Null)
            {
                mementoImagesSlots[0].enabled = true;
                mementoImagesSlots[0].sprite = ui.weaponSprites[ui.weaponSprites.IndexOf(ui.activeWeaponIcon.sprite)];
                
                if (il.secondWeapon != null && il.secondWeapon != WeaponPickUp.Weapon.Null)
                {
                    mementoImagesSlots[1].enabled = true;
                    mementoImagesSlots[1].sprite = ui.weaponSprites[ui.weaponSprites.IndexOf(ui.secondWeaponIcon.sprite)];
                }
                else
                    mementoImagesSlots[1].sprite = ui.emptyInventorySlotIcon;
            }
            else
            {
                mementoImagesSlots[0].sprite = ui.emptyInventorySlotIcon;
                mementoImagesSlots[1].sprite = ui.emptyInventorySlotIcon;
            }
            
            // items
            availableTools.Clear();
            for (int i = 0; i < il.savedTools.Count; i++)
            {
                if (il.savedTools[i].amount > 0)
                    availableTools.Add(i);
            }
            
            for (int i = 2; i < mementoImagesSlots.Count; i++)
            {
                if (i < availableTools.Count + 2)
                {
                    mementoImagesSlots[i].enabled = true;
                    mementoImagesSlots[i].sprite = ui.toolsSprites[availableTools[i - 2]];
                }
                else
                {
                    mementoImagesSlots[i].sprite = ui.emptyInventorySlotIcon;
                }
            }   
        }
        else if (type == InventoryType.Memento)
        {
            if (psc.playerSkills.Count <= 0)
            {
                for (var index = 0; index < mementoImagesSlots.Count; index++)
                {
                    var img = mementoImagesSlots[index];
                    img.sprite = ui.emptyInventorySlotIcon;
                }
            }
            else
            {
                for (int i = 0; i < mementoImagesSlots.Count; i++)
                {
                    if (i < psc.playerSkills.Count)
                    {
                        mementoImagesSlots[i].enabled = true;
                        mementoImagesSlots[i].sprite = psc.playerSkills[i].image;
                    }
                    else
                    {
                        mementoImagesSlots[i].sprite = ui.emptyInventorySlotIcon;
                    }
                }   
//                SelectMemento(0);
            }   
        }
        else if (type == InventoryType.Quests)
        {
            ShowQuests();
        }
        else if (type == InventoryType.QuestItems)
        {
            if (gm.permanentPickupsTakenIndexesInHub.Count > 0)
            {
                mementoImagesSlots[0].enabled = true;
                mementoImagesSlots[0].sprite = il.questItemsList[9].sprite;
            }
            else
            {
                mementoImagesSlots[0].sprite = ui.emptyInventorySlotIcon;
            }
            
            if (il.goldenKeysAmount > 0)
            {
                mementoImagesSlots[1].enabled = true;
                mementoImagesSlots[1].sprite = il.questItemsList[10].sprite;
            }
            else
            {
                mementoImagesSlots[1].sprite = ui.emptyInventorySlotIcon;
            }
            
            if (il.unlockedTracks.Count > 3)
            {
                mementoImagesSlots[2].enabled = true;
                mementoImagesSlots[2].sprite = il.questItemsList[11].sprite;
            }
            else
            {
                mementoImagesSlots[2].sprite = ui.emptyInventorySlotIcon;
            }
            
            if (il.savedQuestItems.Count <= 0)
            {
                for (var index = 3; index < mementoImagesSlots.Count; index++)
                {
                    var img = mementoImagesSlots[index];
                    img.sprite = ui.emptyInventorySlotIcon;
                }
            }
            else
            {
                for (int i = 3; i < mementoImagesSlots.Count; i++)
                {
                    if (i - 3 < il.savedQuestItems.Count)
                    {
                        mementoImagesSlots[i].enabled = true;
                        mementoImagesSlots[i].sprite = il.questItemsList[il.savedQuestItems[i - 3]].sprite;
                    }
                    else
                    {
                        mementoImagesSlots[i].sprite = ui.emptyInventorySlotIcon;
                    }
                }   
//                SelectMemento(0);
            }   
        }
        else if (type == InventoryType.Notes)
        {
            for (int i = 0; i < notesLocals.Count; i ++)
            {
                if (i == gm.language)
                    notesLocals[i].SetActive(true);
                else 
                    notesLocals[i].SetActive(false);
            }
        }
        else if (type == InventoryType.Whispers)
        {
            var newLevelInfo = "";
            var newWhispers = "";
            var newWhispers2 = "";

            var levelEffectString = "";
            int floor = GutProgressionManager.instance.playerFloor;
            switch (gm.language)
            {
                case 0:
                    if (floor >= -3)
                        levelEffectString = "It's dark here and smells rust";
                    else if (floor >= -6)
                        levelEffectString = "It's dark and smells like poison";
                    else if (floor >= -9)
                        levelEffectString = "It is light and cold here";
                    else if (floor >= -20)
                        levelEffectString = "It is light and smells of blood";

                    newLevelInfo = "I'm on " + floor +
                                   " floor. " + levelEffectString +
                                   ". ";

                    newWhispers = "I hear whispers everywhere. As if the walls themselves are crying out to me:";

                    if (il.badReputaion < 2)
                        newWhispers2 = "You are good meat. Do not fight. Behave well and I will love you";
                    else if (il.badReputaion < 3)
                        newWhispers2 = "You're risking. I thought we were friends. Why are you harming others? They don’t like it. The meat will demand more gold from you. More meat will want to kill you. Is this what you want?";
                    else if (il.badReputaion < 4)
                        newWhispers2 = "I despise you.. You are a killer. Why are you doing this? All bets are now even higher. In my labyrinths there is now more dangerous evil.";
                    else if (il.badReputaion < 5)
                        newWhispers2 = "You are very bad. Why are you doing this every time? Go away, I won’t let you go any further. Pay your debt to the Golden Tree";
                    else
                        newWhispers2 = "I hate you";
                    break;
                
                case 1:
                    if (floor >= -3)
                        levelEffectString = "Здесь темно и пахнет ржавчиной";
                    else if (floor >= -6)
                        levelEffectString = "Здесь темно и пахнет ядом";
                    else if (floor >= -9)
                        levelEffectString = "Здесь светло и холодно";
                    else if (floor >= -20)
                        levelEffectString = "Здесь светло и пахнет кровью";

                    newLevelInfo = "Я на " + floor +
                                   " этаже. " + levelEffectString +
                                   ". ";
                    
                    newWhispers = "Я всюду слышу шепот. Словно сами стены взывают ко мне: ";

                    if (il.badReputaion < 2)
                        newWhispers2 = "Ты хорошее мясо. Не ссорься ни с кем. Веди себя хорошо и я буду любить тебя";
                    else if (il.badReputaion < 3)
                        newWhispers2 = "Ты рискуешь. Я думала, мы друзья. Почему ты вредишь другим? Им это не нравится. Мясо будет требовать с тебя больше золота. Больше мяса захочет убить тебя. Ты этого хочешь?";
                    else if (il.badReputaion < 4)
                        newWhispers2 = "Ты неприятен мне. Ты убийца. Зачем ты делаешь это? Все ставки теперь еще выше. В моих лабиринтах теперь больше опасного зла.";
                    else if (il.badReputaion < 5)
                        newWhispers2 = "Ты очень плохой. Зачем ты каждый раз делаешь это? Уходи, я все равно не пропущу тебя дальше. Отдай свой долг Золотому Древу";
                    else
                        newWhispers2 = "Я ненавижу тебя";
                    break;
                
                case 2:
                    if (floor >= -3)
                        levelEffectString = "Está oscuro y huele a óxido";
                    else if (floor >= -6)
                        levelEffectString = "Está oscuro y huele a veneno";
                    else if (floor >= -9)
                        levelEffectString = "Hace luz y frío aquí";
                    else if (floor >= -20)
                        levelEffectString = "Es ligero y huele a sangre";

                    newLevelInfo = "Estoy en el " + floor +
                                   " piso. " + levelEffectString +
                                   ". ";
                    
                    newWhispers = "Escucho susurros por todas partes. Es como si las paredes mismas estuviran hablandome:";

                    if (il.badReputaion < 2)
                        newWhispers2 = "Eres buena carne . No pelees. Comportate y te amare.";
                    else if (il.badReputaion < 3)
                        newWhispers2 = "Te estas arriesgando. Pensaba que eramos amigos. por qué dañas a los demas? A ellos nos les gusta. La Carne demanda mas oro de ti. Mas fiambres querran matarte. Es esto lo que buscas?";
                    else if (il.badReputaion < 4)
                        newWhispers2 = "Te desprecio... Eres un asesino. por qué haces esto?  Ahora el precio a pagar es mas alto. La maldad que alberga mi laberinto es ahora mas peligrosa.";
                    else if (il.badReputaion < 5)
                        newWhispers2 = "Eres muy malo. ¿Por qué haces esto todo el tiempo? Vete, no te dejaré ir mas lejos. Paga tu deuda al Arbol Dorado";
                    else
                        newWhispers2 = "Te odio";
                    break;
                
                case 3:
                    if (floor >= -3)
                        levelEffectString = "Es ist dunkel und riecht nach Rost";
                    else if (floor >= -6)
                        levelEffectString = "Es ist dunkel und riecht nach Gift";
                    else if (floor >= -9)
                        levelEffectString = "Hier ist es hell und kalt";
                    else if (floor >= -20)
                        levelEffectString = "Es ist leicht und riecht nach Blut";

                    newLevelInfo = "Ich bin im " + floor +
                                   " stock. " + levelEffectString +
                                   ". ";
                    
                    newWhispers = "Ich höre Geflüster von überall her. Es ist als ob die Wände selbst nach mir heulen:";

                    if (il.badReputaion < 2)
                        newWhispers2 = "Du bist gutes Fleisch. Kämpfe nicht. Benimm dich gut und ich werde dich lieben";
                    else if (il.badReputaion < 3)
                        newWhispers2 = "Du riskierst. Ich dachte wir wären Freunde. Warum schadest du anderen? Sie mögen es nicht. Das Fleisch wird mehr Gold von Ihnen verlangen. Mehr Fleisch wird dich töten wollen. Ist das was du willst?";
                    else if (il.badReputaion < 4)
                        newWhispers2 = "Ich verachte dich. Du bist ein Mörder. Warum tust du das? Alle Wetten sind jetzt noch höher. In meinen Labyrinthen gibt es jetzt gefährlicheres Übel.";
                    else if (il.badReputaion < 5)
                        newWhispers2 = "Du bist sehr schlecht. Warum machst du das jedes Mal? Geh weg, ich werde dich nicht weiter gehen lassen. Zahlen Sie Ihre Schulden an den Goldenen Baum";
                    else
                        newWhispers2 = "ich hasse dich";
                    break;
                
                case 4:
                    if (floor >= -3)
                        levelEffectString = "It's dark here and smells rust";
                    else if (floor >= -6)
                        levelEffectString = "It's dark and smells like poison";
                    else if (floor >= -9)
                        levelEffectString = "It is light and cold here";
                    else if (floor >= -20)
                        levelEffectString = "It is light and smells of blood";

                    newLevelInfo = "I'm on " + floor +
                                   " floor. " + levelEffectString +
                                   ". ";

                    newWhispers = "I hear whispers everywhere. As if the walls themselves are crying out to me:";

                    if (il.badReputaion < 2)
                        newWhispers2 = "You are good meat. Do not fight. Behave well and I will love you";
                    else if (il.badReputaion < 3)
                        newWhispers2 = "You're risking. I thought we were friends. Why are you harming others? They don’t like it. The meat will demand more gold from you. More meat will want to kill you. Is this what you want?";
                    else if (il.badReputaion < 4)
                        newWhispers2 = "I despise you.. You are a killer. Why are you doing this? All bets are now even higher. In my labyrinths there is now more dangerous evil.";
                    else if (il.badReputaion < 5)
                        newWhispers2 = "You are very bad. Why are you doing this every time? Go away, I won’t let you go any further. Pay your debt to the Golden Tree";
                    else
                        newWhispers2 = "I hate you";
                    break;

                case 5:
                    if (floor >= -3)
                        levelEffectString = "Está escuro aqui e cheira a ferrugem";
                    else if (floor >= -6)
                        levelEffectString = "Está escuro e cheira a veneno";
                    else if (floor >= -9)
                        levelEffectString = "Está claro e frio aqui";
                    else if (floor >= -20)
                        levelEffectString = "É leve e cheira a sangue";

                    newLevelInfo = "Eu estou no " + floor +
                                   " Andar. " + levelEffectString +
                                   ". ";

                    newWhispers = "Eu ouço sussurros por toda parte. Como se as próprias paredes estivessem clamando por mim:";

                    if (il.badReputaion < 2)
                        newWhispers2 = "Você é uma boa carne. Não lute. Comporte-se bem e eu vou te amar";
                    else if (il.badReputaion < 3)
                        newWhispers2 = "Você está se arriscando a sua propria morte. Eu pensei que eramos amigos. Por que você está prejudicando os outros? Eles não gostam disso. A carne exigirá mais ouro de você. E Mais carne vai querer te matar. É isso que voce quer? Você quer mesmo morrer? Quer mesmo que ataquemos você?";
                    else if (il.badReputaion < 4)
                        newWhispers2 = "Eu te desprezo, seu nojento .. Você é um assassino, Verme Maldito. Por que você está fazendo isso? Todas as escolhas agora serão ainda mais terriveis. Nos meus labirintos agora existe um mal mais perigoso. Esse mal ira atras de você, pelo que você fez!";
                    else if (il.badReputaion < 5)
                        newWhispers2 = "Você é Terrivel. Você é um MONSTRO. Por que você está fazendo isso toda hora! Vá embora, Vá embora. eu não vou deixar você ir mais longe. Pague pela sua divida com a Árvore de Ouro, pague com a sua Vida.";
                    else
                        newWhispers2 = "Eu te Odeio.";
                    break;
            }
            
            whispersLevelInfo.text = newLevelInfo;
            whispersOfTheGut.text = newWhispers;
            whispersOfTheGut2.text = newWhispers2;
        }
        else if (type == InventoryType.Leaderboard)
        {
            leaderboardStartButton.onClick.Invoke();
        }

        Quaternion newRot = transform.rotation;
        newRot.eulerAngles = new Vector3(0, 0, Random.Range(-15f, 15f));
        if (mementoImageSlot)
            mementoImageSlot.transform.rotation = newRot; 
    }

    public void Close()
    {
        il = ItemsList.instance;
        psc = PlayerSkillsController.instance;
        gm = GameManager.instance;
        ui = UiManager.instance;

        if (type != InventoryType.Quests)
        {
            for (int i = 0; i < mementoImagesSlots.Count; i++)
            {
                mementoImagesSlots[i].sprite = ui.emptyInventorySlotIcon;
            }

            if (selectedMementoMark)
                selectedMementoMark.enabled = false;
            if (selectedMementoInfo)
                selectedMementoInfo.text = "";   
        }
        else
        {
            for (int i = 0; i < ui.questsNames.Count; i++)
            {
                ui.questsNames[i].text = "";
                ui.questsDescriptions[i].text = "";
                ui.questsCompleteMarks[i].gameObject.SetActive(false);
            }
        }
    }

    public void SelectMemento(int index)
    {
        if (index == -1)
        {
            if (selectedMementoMark)
                selectedMementoMark.enabled = false;
            if (selectedMementoInfo)
                selectedMementoInfo.text = "";
            if (mementoImageSlot)
                mementoImageSlot.gameObject.SetActive(false);
        }
        else
        {
            if (mementoImageSlot)
            {
                Quaternion newRot = transform.rotation;
                newRot.eulerAngles = new Vector3(0, 0, Random.Range(-15f, 15f));
                mementoImageSlot.transform.rotation = newRot; 
                mementoImageSlot.gameObject.SetActive(true);   
            }
            if (!gm)
                gm = GameManager.instance;

            if (type == InventoryType.Memento)
            {
                if (psc.playerSkills.Count > index)
                {
                    if (gm.language == 0)
                        selectedMementoInfo.text = psc.playerSkills[index].info;
                    else if (gm.language == 1)
                        selectedMementoInfo.text = psc.playerSkills[index].infoRu;
                    else if (gm.language == 2)
                        selectedMementoInfo.text = psc.playerSkills[index].infoESP;
                    else if (gm.language == 3)
                        selectedMementoInfo.text = psc.playerSkills[index].infoGER;

                    if (mementoImageSlot)
                        mementoImageSlot.sprite = psc.playerSkills[index].image;
                    if (mementoHolderImage)
                        mementoHolderImage.enabled = true;
                }
                else
                {
                    selectedMementoInfo.text = "";
                    
                    if (mementoImageSlot)
                        mementoImageSlot.sprite = ui.emptyInventorySlotIcon;
                    if (mementoHolderImage)
                        mementoHolderImage.enabled = false;
                }
            }
            else if (type == InventoryType.QuestItems)
            {
                switch (index)
                {
                    case 0:

                        if (gm.permanentPickupsTakenIndexesInHub.Count > 0)
                        {
                            selectedMementoInfo.text = il.questItemsList[9].descriptions[gm.language] + " " + gm.permanentPickupsTakenIndexesInHub.Count;
                            /*if (mementoImageSlot)
                                mementoImageSlot.sprite = il.questItemsList[9].sprite;*/   
                        }
                        else
                        {
                            selectedMementoInfo.text = "";
                            /*if (mementoImageSlot)
                                mementoImageSlot.sprite = ui.emptyInventorySlotIcon;*/   
                        }
                        break;
                    case 1:
                    {
                        if (il.goldenKeysAmount > 0)
                        {
                            selectedMementoInfo.text = il.questItemsList[10].descriptions[gm.language] + " " + il.goldenKeysAmount;
                            if (mementoImageSlot)
                                mementoImageSlot.sprite = il.questItemsList[10].sprite;
                        }
                        else
                        {
                            selectedMementoInfo.text = "";
                            if (mementoImageSlot)
                                mementoImageSlot.sprite = ui.emptyInventorySlotIcon;   
                        }
                        break;
                    }
                    case 2:
                    {
                        if (il.unlockedTracks.Count > 3)
                        {
                            selectedMementoInfo.text = il.questItemsList[11].descriptions[gm.language];

                            for (int i = 0; i < il.unlockedTracks.Count; i++)
                            {
                                if (gm.language == 0)
                                    selectedMementoInfo.text += " [" + il.cassetteNames.lines[il.unlockedTracks[i]].engLines[0] + "]; ";
                                else if (gm.language == 1)
                                    selectedMementoInfo.text += " [" + il.cassetteNames.lines[il.unlockedTracks[i]].rusLines[0] + "]; ";
                                else if (gm.language == 2)
                                    selectedMementoInfo.text += " [" + il.cassetteNames.lines[il.unlockedTracks[i]].espLines[0] + "]; ";
                                else if (gm.language == 3)
                                    selectedMementoInfo.text += " [" + il.cassetteNames.lines[il.unlockedTracks[i]].gerLines[0] + "]; ";
                                else if (gm.language == 4)
                                    selectedMementoInfo.text += " [" + il.cassetteNames.lines[il.unlockedTracks[i]].itaLines[0] + "]; ";
                                else if (gm.language == 5)
                                    selectedMementoInfo.text += " [" + il.cassetteNames.lines[il.unlockedTracks[i]].spbrLines[0] + "]; "; 
                            }
                        
                            if (mementoImageSlot)
                                mementoImageSlot.sprite = il.questItemsList[11].sprite;   
                        }
                        else
                        {
                            selectedMementoInfo.text = "";
                            if (mementoImageSlot)
                                mementoImageSlot.sprite = ui.emptyInventorySlotIcon;   
                        }
                        break;
                    }
                    default:
                    {
                        if (il.savedQuestItems.Count + 3 > index)
                        {
                            selectedMementoInfo.text = il.questItemsList[il.savedQuestItems[index - 3]].descriptions[gm.language];
                            if (mementoImageSlot)
                                mementoImageSlot.sprite = il.questItemsList[il.savedQuestItems[index - 3]].sprite;
                        }
                        else
                        {
                            selectedMementoInfo.text = "";
                            if (mementoImageSlot)
                                mementoImageSlot.sprite = ui.emptyInventorySlotIcon;
                        }

                        break;
                    }
                }
            }
            else if (type == InventoryType.Inventory)
            {
                if (il.activeWeapon != WeaponPickUp.Weapon.Null && index == 0)
                {
                    selectedMementoInfo.text = il.activeWeaponDescriptions[gm.language]; 
                    if (mementoImageSlot)
                        mementoImageSlot.sprite = ui.weaponSprites[ui.weaponSprites.IndexOf(ui.activeWeaponIcon.sprite)];
                }

                if (il.secondWeapon != WeaponPickUp.Weapon.Null && index == 1)
                {
                    
                    selectedMementoInfo.text = il.secondWeaponDescriptions[gm.language];
                    if (mementoImageSlot)
                        mementoImageSlot.sprite = ui.weaponSprites[ui.weaponSprites.IndexOf(ui.secondWeaponIcon.sprite)];  
                }

                if (index > 1)
                {
                    if (availableTools.Count > index - 2)
                    {
                        mementoImagesSlots[index].enabled = true;
                        if (il.savedTools[availableTools[index - 2]].known)
                            selectedMementoInfo.text = il.savedTools[availableTools[index - 2]].info[gm.language];
                        else
                            selectedMementoInfo.text = il.savedTools[availableTools[index - 2]].unknownInfo[gm.language];
                        if (mementoImageSlot)
                            mementoImageSlot.sprite = ui.toolsSprites[availableTools[index - 2]];   
                    }
                    else
                    {
                        mementoImagesSlots[index].sprite = ui.emptyInventorySlotIcon;
                        selectedMementoInfo.text = "";
                    }
                }
            }

            selectedMementoMark.enabled = true;
            selectedMementoMark.transform.parent = mementoImagesSlots[index].transform.parent;
            selectedMementoMark.transform.localPosition = Vector3.zero;
            selectedMementoMark.transform.Rotate(Vector3.forward * Random.Range(0, 360), Space.Self);

            switchItemSourse.pitch = Random.Range(0.75f, 1.25f);
            switchItemSourse.Play();
        }
    }

    void ShowQuests() // when open quests window
    {
        int q = 0;
        var qm = QuestManager.instance;
        var ui = UiManager.instance;

        for (int i = 0; i < qm.activeQuestsIndexes.Count; i++)
        {
            if (qm.questData.quests.Count > qm.activeQuestsIndexes[i])
            {
                ui.questsNames[q].text = qm.questData.quests[qm.activeQuestsIndexes[i]].names[gm.language];
                ui.questsDescriptions[q].text = qm.questData.quests[qm.activeQuestsIndexes[i]].descriptions[gm.language];
                ui.questsCompleteMarks[q].gameObject.SetActive(false);
                q++;   
            }
            
            if (q > 9)
                return;
        }

        if (q < 9)
        {
            for (int i = 0; i < qm.completedQuestsIndexes.Count; i++)
            {
                ui.questsNames[q].text = qm.questData.quests[qm.completedQuestsIndexes[i]].names[gm.language];
                ui.questsDescriptions[q].text = qm.questData.quests[qm.completedQuestsIndexes[i]].descriptions[gm.language];
                ui.questsCompleteMarks[q].gameObject.SetActive(true);
                q++;
            
                if (q > 9)
                    return;
            }
        }
        
        if (q < 9 && il.activeCult != PlayerSkillsController.Cult.none)
        {
            string cultName = "";
            string cultDescription = "";

            switch (il.activeCult)
            {
                case PlayerSkillsController.Cult.poisonCult:
                    if (gm.language == 0)
                    {
                        cultName = "Toxic Grave Cult";
                        cultDescription = "I joined the cult of the Toxic Grave. The elder said Poison is my friend now.";
                    }
                    else if (gm.language == 1)
                    {
                        cultName = "Культ Токсичной Могилы";
                        cultDescription = "Я вступил в культ Токсичной Могилы. Старший сказал, что ЯД теперь мой друг.";
                    }
                    else if (gm.language == 2)
                    {
                        cultName = "Secta de la Tumba Toxica";
                        cultDescription = "Me he unido la secta de la Tumba Tóxica. El anciano dice que el VENENO es ahora mi amigo.";
                    }
                    else if (gm.language == 3)
                    {
                        cultName = "Kult des giftigen Grabes";
                        cultDescription = "Ich bin dem Kult des giftigen Grabes beigetreten. Der Älteste sagte das GIFT wäre nun mein Freund.";
                    }
                    else if (gm.language == 4)
                    {
                        cultName = "Culto della Tomba Tossica";
                        cultDescription = "Mi sono unito al culto della Tomba Tossica. L'anziano dice che il VELENO è mio amico ora.";
                    }
                    else if (gm.language == 5)
                    {
                        cultName = "Culto da Sepultura Tóxica";
                        cultDescription = "Entrei para o culto da Sepultura Tóxica. O ancião disse que o veneno é meu amigo agora.";
                    }
                    break;
                case PlayerSkillsController.Cult.fireCult:
                    if (gm.language == 0)
                    {
                        cultName = "The Cult of the Baby of Flames";
                        cultDescription = "Me he unido a la secta de los Bebes de las Llamas. El anciano dice que el FUEGO es ahora mi amigo.";
                    }
                    else if (gm.language == 1)
                    {
                        cultName = "Культ Младенца Пламени";
                        cultDescription = "Я вступил в культ Младенца Пламени. Старший сказал, что ОГОНЬ теперь мой друг.";
                    }
                    else if (gm.language == 2)
                    {
                        cultName = "Secta de los Bebes de las Llamas";
                        cultDescription = "Me he unido a la secta de los Bebes de las Llamas. El anciano dice que el FUEGO es ahora mi amigo.";
                    }
                    else if (gm.language == 3)
                    {
                        cultName = "Kult des Kindes der Flammen";
                        cultDescription = "Ich bin dem Kult des Kindes der Flammen beigetreten. Der Älteste sagte das FEUER ist jetzt mein Freund.";
                    }
                    else if (gm.language == 4)
                    {
                        cultName = "Culto del Bambino delle Fiamme";
                        cultDescription = "Mi sono unito al culto del Bambino delle Fiamme. L'anziano dice che il FUCO è mio amico ora.";
                    }
                    else if (gm.language == 5)
                    {
                        cultName = "Culto do Bebê das Chamas";
                        cultDescription = "Entrei para o culto do Bebê das Chamas. O ancião disse que FOGO é agora meu amigo";
                    }
                    break;
                case PlayerSkillsController.Cult.bleedingCult:
                    if (gm.language == 0)
                    {
                        cultName = "Cult of Bloodshed Swallowers";
                        cultDescription = "Me he unido a la secta de los Tragadores de Sangre. El anciano dice que la SANGRE es ahora mi amiga.";
                    }
                    else if (gm.language == 1)
                    {
                        cultName = "Культ Кровопролитных Глотателей";
                        cultDescription = "Я вступил в культ Кровопролитных Глотателей. Старший сказал, что КРОВЬ теперь мой друг.";
                    }
                    else if (gm.language == 2)
                    {
                        cultName = "Secta de los Tragadores de Sangre";
                        cultDescription = "Me he unido a la secta de los Tragadores de Sangre. El anciano dice que la SANGRE es ahora mi amiga.";
                    }
                    else if (gm.language == 3)
                    {
                        cultName = "Kult der Schlucker des vergossenen Blutes";
                        cultDescription = "Ich bin dem Kult der Schlucker des vergossenen Blutes beigetreten. Der Älteste sagte das BLUT ist jetzt mein Freund.";
                    }
                    else if (gm.language == 4)
                    {
                        cultName = "Culto degli Ingoiatori di Sangue";
                        cultDescription = "Mi sono unito al culto degli Ingoiatori di Sangue. L'anziano dice che il SANGUE è mio amico ora.";
                    }
                    else if (gm.language == 5)
                    {
                        cultName = "Culto dos Andorinhas do derramamento de sangue";
                        cultDescription = "Entrei para o culto das andorinhas derramamento de sangue. O ancião disse que SANGUE agora é meu amigo";
                    }
                    break;
                case PlayerSkillsController.Cult.goldCult:
                    if (gm.language == 0)
                    {
                        cultName = "Cult of the Golden Phallus";
                        cultDescription = "I joined the cult of the Golden Phallus. Elder said that I will become rich";
                    }
                    else if (gm.language == 1)
                    {
                        cultName = "Культ Золотого Фаллоса";
                        cultDescription = "Я вступил в культ Золотого Фаллоса. Старший сказал, что я стану богат.";
                    }
                    else if (gm.language == 2)
                    {
                        cultName = "Secta de el Falo Dorado";
                        cultDescription = "Me he unido a la secta de El Falo Dorado. El anciano dice que me volveré rico.";
                    }
                    else if (gm.language == 3)
                    {
                        cultName = "Kult des goldenen Gliedes";
                        cultDescription = "Ich bin dem Kult des goldenen Gliedes beigetreten. Der Älteste sagte, dass ich nun reich werde.";
                    }
                    else if (gm.language == 4)
                    {
                        cultName = "Culto del Fallo Aureo";
                        cultDescription = "Mi sono unito al culto del Fallo Aureo. L'anziano dice che diventerò ricco";
                    }
                    else if (gm.language == 5)
                    {
                        cultName = "Culto ao Falo Dourado";
                        cultDescription = "Entrei para o culto do Falo Dourado. O Élder disse que eu ficarei rico";
                    }
                    break;
            }
            
            ui.questsNames[q].text = cultName;
            ui.questsDescriptions[q].text = cultDescription;
            
            ui.questsCompleteMarks[q].gameObject.SetActive(false);
            q++;
            
            if (q > 9)
                return;
        }

        while (q <= 9)
        {
            ui.questsNames[q].text = "";
            ui.questsDescriptions[q].text = "";
            ui.questsCompleteMarks[q].gameObject.SetActive(false);
            q++;
        }
    }
}