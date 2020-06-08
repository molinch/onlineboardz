import { useRef } from 'react';

const useCreateOnce = creator => {
    const hasBeenCalled = useRef(null);
    if (!hasBeenCalled.current) {
        hasBeenCalled.current = creator();
    }

    return hasBeenCalled.current;
}

const useRunOnce = action => {
    const hasBeenCalled = useRef(false);
    if (!hasBeenCalled.current) {
        action();
        hasBeenCalled.current = true
    }
}

export { useCreateOnce, useRunOnce };