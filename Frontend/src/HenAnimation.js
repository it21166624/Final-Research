import React from "react";
import { motion } from "framer-motion";
import styled from "styled-components";

// Styled Container
const HenWrapper = styled.div`
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 20px;
`;

const HenImage = styled(motion.img)`
  width: 150px;
  height: auto;
`;

const HenAnimation = ({ isWeak }) => {
  return (
    <HenWrapper>
      <HenImage
        src={isWeak ? "/weak-hen.png" : "/healthy-hen.png"} // Add two hen images
        alt="Hen"
        animate={{
          scale: isWeak ? [1, 0.95, 1] : [1, 1.1, 1],
          opacity: isWeak ? 0.6 : 1,
          rotate: isWeak ? [-2, 2, -2] : [0, 0, 0],
        }}
        transition={{
          duration: 2,
          repeat: Infinity,
        }}
      />
    </HenWrapper>
  );
};

export default HenAnimation;
