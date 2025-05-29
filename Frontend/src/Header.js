// src/components/Header.js
import React from "react";
import { NavLink } from "react-router-dom";
import styled from "styled-components";

const HeaderContainer = styled.header`
  position: fixed;
  top: 0;
  width: 100%;
  background: rgb(0, 0, 0);
  padding: 15px 30px;
  z-index: 999;
  display: flex;
  justify-content: space-between;
  align-items: center;
  box-shadow: 0 4px 20px rgba(0, 0, 0, 0.1);

  @media (max-width: 768px) {
    flex-direction: column;
    align-items: flex-start;
    padding: 10px 20px;
  }
`;  

const Logo = styled.div`
  font-size: 1.5rem;
  font-weight: bold;
  color: white;
`;

const NavLinks = styled.nav`
  display: flex;
  gap: 60px;
  justify-content: flex-end;
  flex-wrap: wrap;

  @media (max-width: 768px) {
    flex-direction: column;
    align-items: flex-end;
    gap: 10px;
  }
`;

const StyledLink = styled(NavLink)`
  color: white;
  text-decoration: none;
  font-weight: 500;
  padding-bottom: 4px;
  border-bottom: 2px solid transparent;
  transition: border-color 0.2s ease, color 0.2s ease;

  &.active {
    border-bottom: 2px solid #007bff;
    color: #007bff;
  }

  &:hover {
    color: #007bff;
  }
`;

const Header = () => {
  return (
    <HeaderContainer>
      {/* <Logo>EggTrack</Logo> */}
      <NavLinks>
        <StyledLink to="/dashboard">Dashboard</StyledLink>
        <StyledLink to="/stress">Stress</StyledLink>
        <StyledLink to="/humidity">Humidity</StyledLink>
        <StyledLink to="/temperature">Temperature</StyledLink>
        <StyledLink to="/lightening">Lightning</StyledLink>
      </NavLinks>
    </HeaderContainer>
  );
};

export default Header;
