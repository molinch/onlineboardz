import i18n from "i18next";
import { initReactI18next } from "react-i18next";

// the translations
// (tip move them in a JSON file and import them)
const resources = {
    en: {
        translation: {
            "TicTacToe": "Tic Tac Toe",
            "TicTacToe-Desc": "Game for two players. The player who succeeds in placing three of their marks in a horizontal, vertical, or diagonal row is the winner",

            "Memory": "Memory",
            "Memory-Desc": "Memorization game where you should find identical pictures",

            "GooseGame": "Goose game",
            "GooseGame-Desc": "Two or more players move pieces around a track. Reach the end before any of the other players, while avoiding obstacles",
            
            "FindSameAndTapIt": "Find same and tap it",
            "FindSameAndTapIt-Desc": "Tap as quick as possible the similar picture",
            
            "FindStorytellerCard": "Find storyteller's illustrated card",
            "FindStorytellerCard-Desc": "Storyteller's has 5 illustrated cards. He gives an hint about one of them, other players which one it is",
            
            "CardBattle": "Card battle",
            "CardBattle-Desc": "Win all cards in this classical game",
            
            "Scrabble": "Scrabble",
            "Scrabble-Desc": "Word game where words placed next to each other",
        }
    }
};

i18n
    .use(initReactI18next) // passes i18n down to react-i18next
    .init({
        resources,
        lng: "en",

        keySeparator: false, // we do not use keys in form messages.welcome

        interpolation: {
            escapeValue: false // react already safes from xss
        }
    });

export default i18n;