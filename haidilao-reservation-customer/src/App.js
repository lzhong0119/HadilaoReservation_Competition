import React from 'react';
import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import ReservationPage from './pages/ReservationPage';
import QueuePage from './pages/QueuePage';
import CheckQueuePage from './pages/CheckQueuePage';

function App() {
    return (
        <Routes>
            <Route path="/" element={<HomePage />} />
            <Route path="/reserve" element={<ReservationPage />} />
            <Route path="/queue" element={<QueuePage />} />
            <Route path="/checkQueue" element={<CheckQueuePage />} />
        </Routes>
    );
}

export default App;
