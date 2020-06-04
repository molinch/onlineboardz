import React, { useState } from 'react';

function CardIcon(normalIcon, hoveredIcon, className, text) {
    const [isHovered, setIsHovered] = useState(0);

    const icon = isHovered ?
        (hoveredIcon)
        :(normalIcon)

    const classes = `icon-text ${className}`
    return (
        <div class={classes} onMouseEnter={() => setIsHovered(true)} onMouseLeave={() => setIsHovered(false)}>
            {icon} <span class="text">{text}</span>
        </div>
    );
};

export default CardIcon;