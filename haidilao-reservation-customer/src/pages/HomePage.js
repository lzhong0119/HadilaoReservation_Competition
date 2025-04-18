import React, { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './HomePage.css';
import backgroundImage from '../assets/HomepageBackground.jpeg';
import logo from '../assets/HomepageLogo.webp'; // Import the logo

export default function HomePage() {
    const navigate = useNavigate();

    // Add subtle animation delays
    useEffect(() => {
        document.querySelector('.homepage-logo').style.animationDelay = '0.1s';
        document.querySelector('.homepage-title').style.animationDelay = '0.3s';
        document.querySelector('.btn-reserve').style.animationDelay = '0.5s';
        document.querySelector('.btn-queue').style.animationDelay = '0.6s';
        document.querySelector('.btn-check-queue').style.animationDelay = '0.7s';
    }, []);

    return (
        <div
            className="homepage-container"
            style={{ '--background-img': `url(${backgroundImage})` }}
        >
            <div className="logo-title-container">
                <img
                    src={logo}
                    alt="Haidilao Logo"
                    className="homepage-logo"
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                />
                <h1 className="homepage-title">
                    Welcome to <span style={{ color: '#e30613' }}>Haidilao</span>
                    <br />Reservation System
                </h1>
            </div>
            <div className="homepage-buttons">
                <button
                    className="btn btn-reserve"
                    onClick={() => navigate('/reserve')}
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                >
                    Reserve a Table
                </button>
                <button
                    className="btn btn-queue"
                    onClick={() => navigate('/queue')}
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                >
                    Join the Queue
                </button>
                <button
                    className="btn btn-check-queue"
                    onClick={() => navigate('/checkQueue')}
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                >
                    Check queue
                </button>
            </div>
        </div>
    );
}