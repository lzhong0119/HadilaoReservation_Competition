
import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './QueuePage.css';
import axios from 'axios';
import logo from '../assets/HomepageLogo.webp';

const QueuePage = () => {
    const navigate = useNavigate();
    const [outlets, setOutlets] = useState([]);
    const [formData, setFormData] = useState({
        outletId: '',
        customerName: '',
        contactNumber: '',
        numberOfGuest: 1,
        specialRequest: ''
    });
    const [queueCount, setQueueCount] = useState(0);
    const [errors, setErrors] = useState({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitSuccess, setSubmitSuccess] = useState(false);

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
    }, []);

    const handleOutletChange = async (e) => {
        const outletId = e.target.value;
        setFormData({ ...formData, outletId });

        try {
            const response = await axios.get(`/api/queues/count/${outletId}`);
            setQueueCount(response.data);
        } catch (err) {
            console.error('Failed to get queue count', err);
        }
    };

    const validateForm = () => {
        const newErrors = {};

        if (!formData.customerName.trim()) {
            newErrors.customerName = 'Full name is required';
        }

        if (!formData.contactNumber.trim()) {
            newErrors.contactNumber = 'Contact number is required';
        } else if (!/^[0-9+]{10,15}$/.test(formData.contactNumber)) {
            newErrors.contactNumber = 'Please enter a valid contact number';
        }

        if (!formData.outletId) {
            newErrors.outletId = 'Please select an outlet';
        }

        if (!formData.numberOfGuest || formData.numberOfGuest < 1) {
            newErrors.numberOfGuest = 'Number of guests must be at least 1';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        if (!validateForm()) return;

        setIsSubmitting(true);

        try {
            // Submit queue data
            const response = await axios.post('/api/queues/join', formData);
            const queuePosition = response.data.queuePosition;

            // Get outlet details for WhatsApp message
            const outletResponse = await axios.get(`/api/outlets/${formData.outletId}`);
            const outlet = outletResponse.data;

            // Format phone number
            const formatPhoneNumber = (phone) => {
                const digits = phone.replace(/\D/g, '');
                if (digits.startsWith('0') && digits.length >= 10) {
                    return `+60${digits.substring(1)}`;
                } else if (digits.startsWith('60') && digits.length >= 10) {
                    return `+${digits}`;
                }
                return phone;
            };

            // Send WhatsApp notification
            const whatsappPayload = {
                messaging_product: "whatsapp",
                to: formatPhoneNumber(formData.contactNumber),
                type: "template",
                template: {
                    name: "queue_confirmation",
                    language: { code: "en" },
                    components: [
                        {
                            type: "header",
                            parameters: [
                                {
                                    type: "text",
                                    text: outlet.outletName
                                }
                            ]
                        },
                        {
                            type: "body",
                            parameters: [
                                {
                                    type: "text",
                                    text: formData.customerName
                                },
                                {
                                    type: "text",
                                    text: outlet.outletName
                                },
                                {
                                    type: "text",
                                    text: queuePosition.toString()
                                }
                            ]
                        }
                    ]
                }
            };

            await axios.post(
                'https://graph.facebook.com/v22.0/623097337549396/messages',
                whatsappPayload,
                {
                    headers: {
                        'Authorization': 'Bearer EAAaQ5mej98QBOZBLjUY4simoodxWPMrUZBxpVcpA8VSDbGQMP2DuALzpS4w4xVqv2kEIA7dNQRUvR5xZATOXDHqyG8ShZCkajlb7BO0SSVJEeZBciluAYMeIOflVkSvo0wlI7CKnWW47diK1wTTgEoAnb8bHpn6Y96c0ujBs77kpZA27wQBCGS2VU7KPR8lEhnEZBqmmEZBaxAOkICoh9AzFJC7risYZD',
                        'Content-Type': 'application/json'
                    }
                }
            );

            setSubmitSuccess(true);
            setTimeout(() => navigate('/'), 5000);
        } catch (error) {
            const message = error.response?.data;

            // Show specific message if it's the duplicate phone number case
            setErrors({
                submit:
                    message === "This number is already in queue for the selected outlet."
                        ? message
                        : message?.message || 'Failed to join queue. Please try again.'
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="queue-container">
            <div className="logo-header-container">
                <img
                    src={logo}
                    alt="Haidilao Logo"
                    className="queuePage-logo"
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                />
                <div className="queue-header">
                    <h1>Join Our Queue</h1>
                    <p>Get in line for an amazing hot pot experience !</p>
                </div>
            </div>

            {submitSuccess ? (
                <div className="success-message">
                    <h2>You're in Line!</h2>
                    <p>
                        Your queue position has been reserved.
                        We've sent details to your WhatsApp.
                        Redirecting you to homepage...
                    </p>
                </div>
            ) : (
                <form className="queue-form" onSubmit={handleSubmit}>
                    <div className="form-group full-width">
                        <label>Choose Outlet *</label>
                        <select
                            name="outletId"
                            className="form-control"
                            value={formData.outletId}
                            onChange={handleOutletChange}
                        >
                            <option value="">Select an outlet</option>
                            {outlets.map(o => (
                                <option key={o.outletId} value={o.outletId}>
                                    {o.outletName}
                                </option>
                            ))}
                        </select>
                        {errors.outletId && <div className="error-message">{errors.outletId}</div>}
                    </div>

                    {formData.outletId && (
                        <div className="queue-info">
                            <div className="queue-count">
                                <i className="icon-users"></i>
                                <span>Currently {queueCount} {queueCount === 1 ? 'person' : 'people'} in queue</span>
                            </div>
                            <div className="queue-estimate">
                                <i className="icon-clock"></i>
                                <span>Estimated wait: ~{Math.ceil(queueCount * 2.5)} minutes</span>
                            </div>
                        </div>
                    )}

                    <div className="form-group full-width">
                        <label>Full Name *</label>
                        <input
                            type="text"
                            name="customerName"
                            className="form-control"
                            value={formData.customerName}
                            onChange={(e) => setFormData({ ...formData, customerName: e.target.value })}
                            placeholder="Your name"
                        />
                        {errors.customerName && <div className="error-message">{errors.customerName}</div>}
                    </div>

                    <div className="form-group">
                        <label>Contact Number *</label>
                        <input
                            type="tel"
                            name="contactNumber"
                            className="form-control"
                            value={formData.contactNumber}
                            onChange={(e) => setFormData({ ...formData, contactNumber: e.target.value })}
                            placeholder="+60123456789"
                        />
                        {errors.contactNumber && <div className="error-message">{errors.contactNumber}</div>}
                    </div>

                    <div className="form-group">
                        <label>Number of Guests *</label>
                        <select
                            name="numberOfGuest"
                            className="form-control"
                            value={formData.numberOfGuest}
                            onChange={(e) => setFormData({ ...formData, numberOfGuest: e.target.value })}
                        >
                            {[1, 2, 3, 4, 5, 6, 7, 8].map(num => (
                                <option key={num} value={num}>
                                    {num} {num === 1 ? 'person' : 'people'}
                                </option>
                            ))}
                        </select>
                        {errors.numberOfGuest && <div className="error-message">{errors.numberOfGuest}</div>}
                    </div>

                    <div className="form-group full-width">
                        <label>Special Requests (Optional)</label>
                        <textarea
                            name="specialRequest"
                            className="form-control"
                            value={formData.specialRequest}
                            onChange={(e) => setFormData({ ...formData, specialRequest: e.target.value })}
                            rows="3"
                            placeholder="Dietary restrictions, seating preferences, etc."
                        />
                    </div>

                    {errors.submit && <div className="error-message full-width">{errors.submit}</div>}

                    <button
                        type="submit"
                        className="btn-submit"
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? (
                            <>
                                <span className="spinner"></span>
                                Processing...
                            </>
                        ) : (
                            'Join Queue Now'
                        )}
                    </button>
                </form>
            )}
        </div>
    );
};

export default QueuePage;