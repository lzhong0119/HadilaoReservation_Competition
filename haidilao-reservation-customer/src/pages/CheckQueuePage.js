import React, { useState, useEffect, useRef } from 'react';
import { useNavigate } from 'react-router-dom';
import './CheckQueuePage.css';
import axios from 'axios';
import logo from '../assets/HomepageLogo.webp';
import Snackbar from '@mui/material/Snackbar';
import MuiAlert from '@mui/material/Alert';
import { forwardRef } from 'react';

// For proper Alert component with ref forwarding
const Alert = forwardRef(function Alert(props, ref) {
    return <MuiAlert elevation={6} ref={ref} variant="filled" {...props} />;
});

const CheckQueuePage = () => {
    const navigate = useNavigate();
    const [outlets, setOutlets] = useState([]);
    const [formData, setFormData] = useState({
        outletId: '',
        contactNumber: ''
    });
    const [errors, setErrors] = useState({});
    const [isLoading, setIsLoading] = useState(false);
    const [queueStatus, setQueueStatus] = useState(null);
    const [isConnected, setIsConnected] = useState(false);
    const ws = useRef(null);
    const [snackbar, setSnackbar] = useState({
        open: false,
        message: '',
        severity: 'success' // 'success', 'error', 'warning', 'info'
    });

    useEffect(() => {
        const fetchOutlets = async () => {
            try {
                const response = await axios.get('/api/outlets');
                setOutlets(response.data);
            } catch (error) {
                console.error('Error fetching outlets:', error);
            }
        };
        fetchOutlets();

        return () => {
            if (ws.current) {
                ws.current.close();
            }
        };
    }, []);

    const validateForm = () => {
        const newErrors = {};

        if (!formData.contactNumber.trim()) {
            newErrors.contactNumber = 'Contact number is required';
        } else if (!/^[0-9+]{10,15}$/.test(formData.contactNumber)) {
            newErrors.contactNumber = 'Please enter a valid contact number';
        }

        if (!formData.outletId) {
            newErrors.outletId = 'Please select an outlet';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleCheckQueue = async (e) => {
        e.preventDefault();
        if (!validateForm()) return;

        setIsLoading(true);

        try {
            const response = await axios.get('/api/queues/check', {
                params: {
                    outletId: formData.outletId,
                    contactNumber: formData.contactNumber
                }
            });

            setQueueStatus(response.data);
            connectWebSocket(formData.outletId);
        } catch (error) {
            console.error('Error checking queue:', error);
            setErrors({
                submit: error.response?.data?.message ||
                    'No record found.'
            });
        } finally {
            setIsLoading(false);
        }
    };

    const connectWebSocket = (outletId) => {
        if (ws.current) {
            ws.current.close();
        }

        const wsUrl = `ws://localhost:5057/ws/queue`;
        console.log("WS connecting to", wsUrl);

        ws.current = new WebSocket(wsUrl);
        setIsConnected(false);

        ws.current.onopen = () => {
            setIsConnected(true);
            console.log('WebSocket connected');

            // Send initial data to identify this connection
            ws.current.send(JSON.stringify({
                type: "register",
                outletId: outletId
            }));
        };

        ws.current.onmessage = (event) => {
            const data = JSON.parse(event.data);
            console.log('WebSocket message:', data);

            if (data.type === 'queueUpdate' && data.outletId === outletId) {
                setQueueStatus(prev => ({
                    ...prev,
                    aheadCount: data.waitingCount - (prev.queuePosition - (data.waitingCount - prev.aheadCount))
                }));
            }
        };

        ws.current.onclose = () => {
            setIsConnected(false);
            console.log('WebSocket disconnected');

            // Attempt to reconnect after 5 seconds
            setTimeout(() => {
                if (queueStatus) {
                    console.log('Attempting to reconnect WebSocket');
                    connectWebSocket(outletId);
                }
            }, 5000);
        };

        ws.current.onerror = (error) => {
            console.error('WebSocket error:', error);
        };
    };

    const formatPhoneNumber = (phone) => {
        // Format for display
        const digits = phone.replace(/\D/g, '');
        if (digits.startsWith('60') && digits.length === 11) {
            return `${digits.substring(0, 3)}-${digits.substring(3, 6)} ${digits.substring(6, 9)} ${digits.substring(9)}`;
        }
        return phone;
    };

    // Function to handle leaving the queue
    const handleLeaveQueue = async () => {
        if (window.confirm('Are you sure you want to leave the queue? This action cannot be undone.')) {
            try {
                setIsLoading(true);
                await axios.post('/api/queues/leave', {
                    outletId: formData.outletId,
                    contactNumber: formData.contactNumber
                });

                // Close WebSocket connection
                if (ws.current) {
                    ws.current.close();
                }

                // Reset the queue status
                setQueueStatus(null);

                // Show success message
                setSnackbar({
                    open: true,
                    message: 'You have left the queue successfully',
                    severity: 'success'
                });
            } catch (error) {
                console.error('Error leaving queue:', error);

                // Show error message
                setSnackbar({
                    open: true,
                    message: error.response?.data?.message || 'Failed to leave queue',
                    severity: 'error'
                });
            } finally {
                setIsLoading(false);
            }
        }
    };

    return (
        <div className="check-queue-container">
            <div className="logo-header-container">
                <img
                    src={logo}
                    alt="Haidilao Logo"
                    className="checkQueue-logo"
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                />
                <div className="check-queue-header">
                    <h1>Check Your Queue</h1>
                    <p>Real-time updates on your waiting status</p>
                </div>
            </div>

            {queueStatus ? (
                <div className="queue-status-container">
                    <div className="status-card">
                        <div className="status-header">
                            <h2>{queueStatus.CustomerName}</h2>
                            <span className="connection-status">
                                {isConnected ? (
                                    <>
                                        <span className="connected-dot"></span> Live Updates
                                    </>
                                ) : (
                                    <>
                                        <span className="disconnected-dot"></span> Offline
                                    </>
                                )}
                            </span>
                        </div>

                        <div className="status-details">
                            <div className="detail-row">
                                <span className="detail-label">Outlet:</span>
                                <span className="detail-value">{queueStatus.outletName}</span>
                            </div>
                            <div className="detail-row">
                                <span className="detail-label">Contact:</span>
                                <span className="detail-value">{formatPhoneNumber(formData.contactNumber)}</span>
                            </div>
                            <div className="detail-row">
                                <span className="detail-label">Party Size:</span>
                                <span className="detail-value">{queueStatus.numberOfGuest}</span>
                            </div>
                        </div>

                        <div className="queue-position">
                            <div className="position-number">{queueStatus.queuePosition}</div>
                            <div className="position-label">Your Queue Number</div>
                        </div>

                        <div className="queue-stats">
                            <div className="stat-item">
                                <div className="stat-value">{queueStatus.aheadCount}</div>
                                <div className="stat-label">Parties Ahead</div>
                            </div>
                            <div className="stat-item">
                                <div className="stat-value">~{queueStatus.estimatedWaitMinutes}</div>
                                <div className="stat-label">Minutes Wait</div>
                            </div>
                        </div>

                        <div className="status-footer">
                            <p>We'll notify you when your table is ready</p>
                            <button
                                className="btn-back"
                                onClick={() => setQueueStatus(null)}
                            >
                                Check Another Queue
                            </button>
                            <button
                                className="btn-leave"
                                onClick={handleLeaveQueue}
                                disabled={isLoading}
                            >
                                {isLoading ? 'Leaving...' : 'Leave Queue'}
                            </button>
                        </div>
                    </div>
                </div>
            ) : (
                <form className="check-queue-form" onSubmit={handleCheckQueue}>
                    <div className="form-group full-width">
                        <label>Select Outlet *</label>
                        <select
                            name="outletId"
                            className="form-control"
                            value={formData.outletId}
                            onChange={(e) => setFormData({ ...formData, outletId: e.target.value })}
                        >
                            <option value="">Choose your outlet</option>
                            {outlets.map(o => (
                                <option key={o.outletId} value={o.outletId}>
                                    {o.outletName}
                                </option>
                            ))}
                        </select>
                        {errors.outletId && <div className="error-message">{errors.outletId}</div>}
                    </div>

                    <div className="form-group full-width">
                        <label>Contact Number *</label>
                        <input
                            type="tel"
                            name="contactNumber"
                            className="form-control-contact"
                            value={formData.contactNumber}
                            onChange={(e) => setFormData({ ...formData, contactNumber: e.target.value })}
                            placeholder="+60123456789"
                        />
                        {errors.contactNumber && <div className="error-message">{errors.contactNumber}</div>}
                    </div>

                    {errors.submit && <div className="error-message full-width">{errors.submit}</div>}

                    <button
                        type="submit"
                        className="btn-submit"
                        disabled={isLoading}
                    >
                        {isLoading ? (
                            <>
                                <span className="spinner"></span>
                                Checking...
                            </>
                        ) : (
                            'Check My Queue'
                        )}
                    </button>
                </form>
            )}
            <Snackbar
                open={snackbar.open}
                autoHideDuration={6000}
                onClose={() => setSnackbar({ ...snackbar, open: false })}
            >
                <Alert
                    onClose={() => setSnackbar({ ...snackbar, open: false })}
                    severity={snackbar.severity}
                    sx={{ width: '100%' }}
                >
                    {snackbar.message}
                </Alert>
            </Snackbar>
        </div>
    );
};

export default CheckQueuePage;