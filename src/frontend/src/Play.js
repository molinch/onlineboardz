import React, { useState, useEffect } from 'react';
import { Card, Modal, InputNumber, Switch } from 'antd';
import { HeartOutlined, HeartFilled, SmileOutlined, SmileFilled } from '@ant-design/icons';
import CardIcon from './CardIcon';
import { GameTypeInfo } from './games/GameType';
import { useTranslation, Trans } from 'react-i18next';
import getGameData from './games/GameData';
import { navigate } from "@reach/router";
import config from './config';

const { Meta } = Card;
const ChooseGame = props =>
    CardIcon(
        (<SmileOutlined key="join" onClick={props.onClick} />),
        (<SmileFilled key="join" onClick={props.onClick} />),
        "icon-join", "Join"
    );
const Favorite = props =>
    CardIcon(
        (<HeartOutlined key="heart" onClick={props.onClick} />),
        (<HeartFilled key="heart" onClick={props.onClick} />),
        "icon-favorite", "Favorite"
    );

const defaultGameOptions = { hasMaxPlayers: false, maxPlayers: null, specificDuration: false, duration: null, isOpen: true };

const Play = ({ user, fetchWithUi }) => {
    const { t } = useTranslation();
    const [gameTypes, setGameTypes] = useState([]);
    const [error, setError] = useState(<></>);
    const [selectedGame, setSelectedGame] = useState(null);
    const [gameOptions, setGameOptions] = useState(defaultGameOptions);
    useEffect(() => {
        if (!user) return;

        (async () => {
            const gameData = getGameData(fetchWithUi);
            const response = await gameData.getGameTypes();
            if (response.error) {
                setError(response.error);
                return;
            }
            setGameTypes(response);
        })();
    }, [user, fetchWithUi]);

    const onGameChosen = (gameType, gameTypeInfo) => {
        setSelectedGame({ gameType, gameTypeInfo });
    }

    const onFavorite = gameTypeInfo => {
        /*
        const response = await fetchWithUi.put(
            `${config.GameServiceUri}/favorites`,
            { gameType: selectedGame.type, players: numberOfPlayers, duration }
        );
        */
    }

    const confirmPlay = async () => {
        const response = await fetchWithUi.patch(
            `${config.GameServiceUri}/gameProposals/joinAny`,
            {
                gameType: selectedGame.gameType.gameType,
                maxPlayers: gameOptions.hasMaxPlayers ? gameOptions.maxPlayers : null,
                duration: gameOptions.specificDuration ? gameOptions.duration : null,
                isOpen: gameOptions.isOpen,
            }
        );
        if (response.error) {
            setError(response.error);
            reset();
            return;
        }
        
        const gameTypeInfo = GameTypeInfo.ById(response.metadata.gameType);
        navigate(`/games/${gameTypeInfo.name}/${response.id}`);
        reset();
    }

    const cancelPlay = () => {
        reset();
    }

    const reset = () => {
        setSelectedGame(null);
        setGameOptions(defaultGameOptions);
    };

    let playModalContent = (<></>);
    const gameType = selectedGame?.gameType;
    if (gameType) {
        let players = (
            <div>
                {t("NumberOfPlayers")}
                {gameType.minPlayers}
            </div>
        );
        if (gameType.minPlayers !== gameType.maxPlayers) {
            const playersSwitch = gameOptions.hasMaxPlayers
                ?
                    (
                        <div>
                            {t("GameOptionAtMostPlayersStart")}
                            <InputNumber
                                min={gameType.minPlayers}
                                max={gameType.maxPlayers}
                                defaultValue={gameType.minPlayers}
                                onChange={p => setGameOptions({...gameOptions, maxPlayers: p})}
                            />
                            {t("GameOptionAtMostPlayersEnd")}
                        </div>
                    )
                :
                    (
                        <></>
                    );

            players = 
                (
                    <div>
                        <Switch
                            defaultChecked
                            onChange={o => setGameOptions({...gameOptions, hasMaxPlayers: !o})}
                        />
                        {t("GameOptionPlayer")}
                        {playersSwitch}
                    </div>
                );
        }

        const specificDuration = gameOptions.specificDuration
            ?
                (
                    <div>
                        {t("GameOptionAtMostDurationStart")}
                        <InputNumber 
                            min={Math.trunc(gameType.defaultDuration/2)} 
                            max={gameType.defaultDuration*2} 
                            defaultValue={gameType.defaultDuration} 
                            onChange={d => setGameOptions({...gameOptions, duration: d})}
                        />
                        {t("GameOptionAtMostDurationEnd")}
                    </div>
                )
            : (<></>);
        const duration = (
            <div>
                <Switch
                    defaultChecked
                    onChange={o => setGameOptions({...gameOptions, specificDuration: !o})}
                />
                {t("GameOptionDuration")}
                {specificDuration}
            </div>
        );

        const open = (
            <div>
                <Switch
                    defaultChecked
                    onChange={o => setGameOptions({...gameOptions, isOpen: o})}
                /> 
                {t("GameOptionIsOpen")}
            </div>
        );

        playModalContent = (
            <>
            <h3>{t("GameOptions")}</h3>
            {players}
            {duration}
            {open}
            </>
        );
    }
    const playModalVisible = selectedGame !== null;
    return (
        <div>
            <h1>{t('ChooseGame')}</h1>
            {error}

            <div className="seek-cards">
                {gameTypes.map(g => {
                    const gameTypeInfo = GameTypeInfo.ById(g.gameType);
                    return (
                        <Card
                            key={gameTypeInfo.name}
                            style={{ width: 300 }}
                            cover={
                                <img
                                    alt="logo"
                                    src={gameTypeInfo.logo}
                                />
                            }
                            actions={[
                                <ChooseGame onClick={() => onGameChosen(g, gameTypeInfo)} />,
                                <Favorite onClick={() => onFavorite(gameTypeInfo)} />
                            ]}
                        >
                            <Meta
                                title={t(gameTypeInfo.name)}
                                description={t(gameTypeInfo.name + "-Desc")}
                            />
                        </Card>
                    );
                })}
            </div>

            <Modal
                title={t(selectedGame?.gameTypeInfo?.name)}
                visible={playModalVisible}
                onOk={confirmPlay}
                onCancel={cancelPlay}
            >
                {playModalContent}
            </Modal>

        </div>
    );
}

export default Play;