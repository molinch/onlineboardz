import React, { useState, useEffect, useCallback } from 'react';
import './TicTacToe.css';
import { useTranslation } from 'react-i18next';
import Chat from '../Chat';
import config from '../../config';
import GameStatus from '../GameStatus';
import PlayerGameStatus from '../PlayerGameStatus';

const stepNameFromBool = (value) => {
    if (value === null) return '';
    return value ? 'X' : 'O';
};

const stepNameFromPlayer = (player) => stepNameFromBool(player.playOrder === 0);

const Square = ({ value, onClick }) => {
    return (
        <button
            className="square"
            onClick={onClick}
        >
            {value}
        </button>
    );
};

const Board = ({ squares, onClick }) => {
    const renderSquare = i => {
        return (
            <Square
                value={stepNameFromBool(squares[i])}
                onClick={() => onClick(i)}
            />
        );
    };

    return (
        <div>
            <div className="board-row">
                {renderSquare(0)}
                {renderSquare(1)}
                {renderSquare(2)}
            </div>
            <div className="board-row">
                {renderSquare(3)}
                {renderSquare(4)}
                {renderSquare(5)}
            </div>
            <div className="board-row">
                {renderSquare(6)}
                {renderSquare(7)}
                {renderSquare(8)}
            </div>
        </div>
    );
}

const TicTacToe = ({gameId, user, fetchWithUi, passedGame, gameNotificationClient, gameReachabilityChecker, setError}) => {
    const processGame = useCallback(game => {
        if (!user || !game) {
            return {
                game: null,
                opponent: null,
                currentPlayer: null,
                history: [Array(9).fill(null)],
            };
        }
        const opponent = game.players.find(p => p.id !== user.id);
        const history = recomposeHistory(game);
        const currentPlayer = game.players.find(p => (history.length -1) % 2 ===  p.playOrder); // -1 since we have an initial zero item
        
        setGameData({
            game,
            opponent,
            currentPlayer,
            history,
        });
    }, [user]);

    const { t } = useTranslation();
    const [gameData, setGameData] = useState(processGame(passedGame));
    const [stepNumber, setStepNumber] = useState(null);
    const {game, opponent, currentPlayer, history} = gameData || {};

    const recomposeHistory = game => {
        let previousHistoryEntry = Array(9).fill(null);
        const history = [];
        history.push(previousHistoryEntry);

        if (game) {
            game.cells
                .map((cell, index) => {
                    if (!cell) return null;

                    return {
                        nr: cell.number,
                        idx: index,
                        step: cell.number % 2
                    }
                })
                .filter(cell => cell != null)
                .sort((a, b) => {
                    if (a.nr < b.nr) {
                        return -1;
                    } else if (b.nr > a.r) {
                        return 1;
                    } else {
                        return 0;
                    }
                })
                .forEach(item => {
                    previousHistoryEntry = [...previousHistoryEntry];
                    previousHistoryEntry[item.idx] = item.step;
                    history.push(previousHistoryEntry);
                });
        }
        return history;
    };

    useEffect(() => {
        if (!user) return;

        const removeHandlers = [
            gameNotificationClient.addHandler('PlayerAdded', game => {
                processGame(game);
            }),
            gameNotificationClient.addHandler('GameStarted', game => {
                processGame(game);
            }),
            gameNotificationClient.addHandler('GameStepAdded', game => {
                processGame(game);
            })
        ];

        return () => {
            removeHandlers.forEach(removeHandler => removeHandler());
        };
    }, [gameNotificationClient, user, processGame]);

    const fetchGame = useEffect(() => {
        (async () => {
            if (!user) return;

            const response = await fetchWithUi.get(`${config.GameServiceUri}/games/${gameId}`);
            if (response.error) {
                setError(response.error);
                return;
            }
            processGame(response);
        })();
    }, [user, fetchWithUi, gameId, processGame, setError]);

    // that could be better achieved via SignalR/WebSocket I guess
    gameReachabilityChecker.addReachableChangedHandler(isReachable => {
        fetchGame();
    });

    let status = t('Waiting for players...');
    let players = [];
    let movesHistory = null;
    if (game) {
        players = game.players.map(p => {
            let statusText = "";
            switch (p.status) {
                case PlayerGameStatus.Draw:
                case PlayerGameStatus.Won:
                case PlayerGameStatus.Lost:
                    statusText = t(`PlayerGameStatus_${p.status}`);
                    break;
                default:
                    statusText = "";
                    break;
            }
            return `${p.name} ${statusText} (${stepNameFromPlayer(p)})`;
        }).join(", ");
        switch (game.status) {
            case GameStatus.InGame:
                status = t('TicTacToe_NextPlayer', { player: `${currentPlayer.name} (${stepNameFromPlayer(currentPlayer)})` });
                break;
            case GameStatus.TimedOut:
            case GameStatus.Finished:
                status = t('TicTacToe_GameEnded');
                break;
            default:
                break;
        }

        if (game.status === GameStatus.TimedOut || game.status === GameStatus.Finished) {
            const moves = history.map((step, stepNumber) => {
                const desc = stepNumber ?
                    t('TicTacToe_BackToMoveNr') + stepNumber :
                    t('TicTacToe_BackToBeginning');
                return (
                    <li key={stepNumber}>
                        <button onClick={() => setStepNumber(stepNumber)}>{desc}</button>
                    </li>
                );
            });

            movesHistory = (
                <div>{t('TicTacToe_MovesHistory')}{moves}</div>
            );
        }
    }

    const sendStep = async (cellIndex) => {
        const data = { gameId, cellIndex };
        const response = await fetchWithUi.patch(`${config.GameServiceUri}/tictactoe/`, data);
        if (response.error) {
            setError(response.error);
            return;
        }
        processGame(response);
    }

    const handleClick = async i => {
        if (!user || !game || game.status !== 1) return;
        if (currentPlayer.id !== user.id) return;

        const current = history[history.length - 1];
        if (current[i]) {
            return;
        }

        const squares = current.slice();
        squares[i] = currentPlayer.playOrder === 0;

        setGameData({
            ...gameData,
            history: history.concat([squares]),
            currentPlayer: opponent,
        });

        sendStep(i);
    };

    const current = stepNumber === null ? history[history.length-1] : history[stepNumber];
    return (
        <div className="tictactoe">
            <div className="game-board">
                <Board
                    squares={current}
                    onClick={i => handleClick(i)}
                />
            </div>
            <div className="game-info">
                <div>Players: {players}</div>
                <div className="status">{status}</div>
                {movesHistory}
            </div>
        </div>
    );
}

const TicTacToeGame = ({gameId, user, fetchWithUi, game, gameNotificationClient, gameReachabilityChecker, setError}) => {
    return (
        <>
            <div style={{float: 'left', width: '70%', backgroundColor: 'lightblue'}}>
                <TicTacToe
                    gameId={gameId}
                    user={user}
                    fetchWithUi={fetchWithUi}
                    passedGame={game}
                    gameNotificationClient={gameNotificationClient}
                    gameReachabilityChecker={gameReachabilityChecker}
                    setError={setError}
                />
            </div>
            <div style={{float: 'right', width: '30%', backgroundColor: '#aaa'}}>
                <Chat></Chat>
            </div>
        </>
    );
}

export default TicTacToeGame;