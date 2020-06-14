import React, { useState } from 'react';
import './TicTacToe.css';
import { useTranslation } from 'react-i18next';

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
                value={squares[i]}
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

const TicTacToe = () => {
    const { t } = useTranslation();
    const [state, setState] = useState({
        history: [{
            squares: Array(9).fill(null),
        }],
        xIsNext: true,
        winner: null,
        stepNumber: 0,
    });

    const handleClick = i => {
        const history = state.history;
        const current = history[history.length - 1];
        if (state.winner || current.squares[i]) {
            return;
        }

        const squares = current.squares.slice();
        squares[i] = state.xIsNext ? 'X' : 'O';
        setState({
            history: history.concat([{
                squares: squares,
            }]),
            xIsNext: !state.xIsNext,
            stepNumber: history.length,
            winner: calculateWinner(squares)
        });
    };

    const jumpTo = stepNumber => {
        setState({
            ...state,
            stepNumber: stepNumber
        });
    };

    const calculateWinner = squares => {
        const lines = [
            [0, 1, 2],
            [3, 4, 5],
            [6, 7, 8],
            [0, 3, 6],
            [1, 4, 7],
            [2, 5, 8],
            [0, 4, 8],
            [2, 4, 6],
        ];
        for (let i = 0; i < lines.length; i++) {
            const [a, b, c] = lines[i];
            if (squares[a] && squares[a] === squares[b] && squares[a] === squares[c]) {
                return squares[a];
            }
        }
        return null;
    };

    const history = state.history;
    const current = history[state.stepNumber];
    const winner = state.winner;
    let status;
    if (winner) {
        status = t('TicTacToe_Won', { winner: winner });
    } else {
        status = t('TicTacToe_NextPlayer') + (state.xIsNext ? 'X' : 'O');
    }

    let movesHistory = null;
    if (winner) {
        const moves = history.map((step, move) => {
            const desc = move ?
                t('TicTacToe_BackToMoveNr') + move :
                t('TicTacToe_BackToBeginning');
            return (
                <li key={move}>
                    <button onClick={() => jumpTo(move)}>{desc}</button>
                </li>
            );
        });

        movesHistory = (
            <div>{t('TicTacToe_MovesHistory')}{moves}</div>
        );
    }

    return (
        <div className="tictactoe">
            <div className="game-board">
                <Board
                    squares={current.squares}
                    onClick={i => handleClick(i)}
                />
            </div>
            <div className="game-info">
                <div className="status">{status}</div>
                {movesHistory}
            </div>
        </div>
    );
}

export default TicTacToe;