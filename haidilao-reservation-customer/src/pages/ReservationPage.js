import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import './ReservationPage.css';
import axios from 'axios';
import logo from '../assets/HomepageLogo.webp';

axios.get('http://localhost:5057/api/outlets')
    .then(response => console.log(response.data))
    .catch(error => console.error('Direct call failed:', error));

const ReservationPage = () => {
    const navigate = useNavigate();
    const [outlets, setOutlets] = useState([]);
    const [formData, setFormData] = useState({
        customerName: '',
        contactNumber: '',
        numberOfGuest: 2,
        outletId: '',
        reservationDateTime: '', // Combined field
        specialRequest: ''
    });
    const [errors, setErrors] = useState({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitSuccess, setSubmitSuccess] = useState(false);

    useEffect(() => {
        // Fetch outlets from API
        const fetchOutlets = async () => {
            try {
                const response = await axios.get('/api/outlets');
                setOutlets(response.data);
                if (response.data.length > 0) {
                    setFormData(prev => ({ ...prev, outletId: response.data[0].outletId }));
                }
            } catch (error) {
                console.error('Error fetching outlets:', error);
            }
        };

        fetchOutlets();
    }, []);

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

        if (!formData.reservationDateTime) {
            newErrors.reservationDateTime = 'Date and time are required';
        } else {
            const selectedDate = new Date(formData.reservationDateTime);
            if (selectedDate < new Date()) {
                newErrors.reservationDateTime = 'Must be in the future';
            } else {
                const hours = selectedDate.getHours();
                const minutes = selectedDate.getMinutes();
                const timeInMinutes = hours * 60 + minutes;

                const start = 11 * 60; // 11:00 AM
                const midnight = 24 * 60; // 12:00 AM next day
                const end = 1 * 60; // 1:00 AM

                const isValidTime =
                    (timeInMinutes >= start && timeInMinutes < midnight) || // 11:00–23:59
                    (timeInMinutes >= 0 && timeInMinutes < end);           // 00:00–00:59

                if (!isValidTime) {
                    newErrors.reservationDateTime = 'Reservations allowed only between 11:00 AM and 1:00 AM';
                }
            }

        }

        if (!formData.numberOfGuest || formData.numberOfGuest < 1) {
            newErrors.numberOfGuest = 'Number of guests must be at least 1';
        }

        setErrors(newErrors);
        return Object.keys(newErrors).length === 0;
    };

    const handleChange = (e) => {
        const { name, value } = e.target;

        // Parse outletId to number, keep others as string
        const parsedValue = name === 'outletId' ? Number(value) : value;

        setFormData(prev => ({
            ...prev,
            [name]: parsedValue
        }));

        if (errors[name]) {
            setErrors(prev => ({
                ...prev,
                [name]: ''
            }));
        }
    };


    const handleSubmit = async (e) => {
        e.preventDefault();

        if (!validateForm()) return;

        setIsSubmitting(true);

        try {
            const payload = {
                OutletId: Number(formData.outletId),
                CustomerName: formData.customerName,
                ContactNumber: formData.contactNumber,
                NumberOfGuest: Number(formData.numberOfGuest),
                ReservationDateTime: formData.reservationDateTime,
                SpecialRequest: formData.specialRequest || null,
            };

            console.log('Final payload:', payload);
            const response = await axios.post('/api/reservations', payload);

            // Get the outlet details for the WhatsApp message
            const outletResponse = await axios.get(`/api/outlets/${formData.outletId}`);
            const outlet = outletResponse.data;

            // Format the reservation date
            const reservationDate = new Date(formData.reservationDateTime);
            const formattedDate = reservationDate.toLocaleString('en-MY', {
                weekday: 'long',
                day: 'numeric',
                month: 'long',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit'
            });

            // Format phone number properly
            const formatPhoneNumber = (phone) => {
                // Remove all non-digit characters
                const digits = phone.replace(/\D/g, '');

                // Handle Malaysian numbers (01161708616 → +601161708616)
                if (digits.startsWith('0') && digits.length >= 10) {
                    return `+60${digits.substring(1)}`;
                }
                // Handle if already has country code (601161708616 → +601161708616)
                else if (digits.startsWith('60') && digits.length >= 10) {
                    return `+${digits}`;
                }
                // Return as-is if already international format
                return phone;
            };

            const recipientNumber = formatPhoneNumber(formData.contactNumber);

            // Prepare WhatsApp message payload
            const whatsappPayload = {
                messaging_product: "whatsapp",
                to: recipientNumber,
                type: "template",
                template: {
                    name: "reservation_pending",
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
                                    text: formattedDate
                                }
                            ]
                        }
                    ]
                }
            };

            console.log('WhatsApp payload:', whatsappPayload);

            // Send WhatsApp message
            try {
                const waResponse = await axios.post(
                    'https://graph.facebook.com/v22.0/623097337549396/messages',
                    whatsappPayload,
                    {
                        headers: {
                            'Authorization': 'Bearer EAAaQ5mej98QBOZBLjUY4simoodxWPMrUZBxpVcpA8VSDbGQMP2DuALzpS4w4xVqv2kEIA7dNQRUvR5xZATOXDHqyG8ShZCkajlb7BO0SSVJEeZBciluAYMeIOflVkSvo0wlI7CKnWW47diK1wTTgEoAnb8bHpn6Y96c0ujBs77kpZA27wQBCGS2VU7KPR8lEhnEZBqmmEZBaxAOkICoh9AzFJC7risYZD',
                            'Content-Type': 'application/json'
                        }
                    }
                );

                if (waResponse.status === 200) {
                    console.log('✅ WhatsApp message sent successfully.');
                    console.log('Response from WhatsApp:', waResponse.data);
                } else {
                    console.error('❌ WhatsApp responded with non-200:', waResponse.status);
                    console.error(waResponse.data);
                }
            } catch (waError) {
                console.error('❌ Error sending WhatsApp message:', waError.message);
                if (waError.response) {
                    console.error('Status:', waError.response.status);
                    console.error('Error response:', waError.response.data);
                }
            }

            setSubmitSuccess(true);
            setTimeout(() => navigate('/'), 5000);
        } catch (error) {
            console.error('Full error:', {
                message: error.message,
                response: error.response?.data
            });
            setErrors({
                submit: error.response?.data?.message ||
                    error.response?.data?.error?.message ||
                    'Failed to create reservation or send confirmation'
            });
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="reservation-container">
            <div className="logo-header-container">
                <img
                    src={logo}
                    alt="Haidilao Logo"
                    className="reservationPage-logo"
                    style={{ opacity: 0, animation: 'fadeIn 0.6s forwards' }}
                />
                <div className="reservation-header">
                    <h1>Make a Reservation</h1>
                    <p>Book your table at HaiDiLao Hot Pot</p>
                </div>
            </div>


            {submitSuccess ? (
                <div className="success-message">
                    <h2>Reservation Successful!</h2>
                    <p>You will receive a confirmation shortly. Redirecting to homepage...</p>
                </div>
            ) : (
                <form className="reservation-form" onSubmit={handleSubmit}>
                    <div className="form-group full-width">
                        <label htmlFor="customerName">Full Name *</label>
                        <input
                            type="text"
                            id="customerName"
                            name="customerName"
                            className="form-control"
                            value={formData.customerName}
                            onChange={handleChange}
                            placeholder="Enter your full name"
                        />
                        {errors.customerName && <div className="error-message">{errors.customerName}</div>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="contactNumber">Contact Number *</label>
                        <input
                            type="tel"
                            id="contactNumber"
                            name="contactNumber"
                            className="form-control"
                            value={formData.contactNumber}
                            onChange={handleChange}
                            placeholder="e.g., +60123456789"
                        />
                        {errors.contactNumber && <div className="error-message">{errors.contactNumber}</div>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="numberOfGuest">Number of Guests *</label>
                        <select
                            id="numberOfGuest"
                            name="numberOfGuest"
                            className="form-control"
                            value={formData.numberOfGuest}
                            onChange={handleChange}
                        >
                            {[1, 2, 3, 4, 5, 6, 7, 8, 9, 10].map(num => (
                                <option key={num} value={num}>{num} {num === 1 ? 'person' : 'persons'}</option>
                            ))}
                        </select>
                        {errors.numberOfGuest && <div className="error-message">{errors.numberOfGuest}</div>}
                    </div>

                    <div className="form-group">
                        <label htmlFor="outletId">Outlet *</label>
                        <select
                            id="outletId"
                            name="outletId"
                            className="form-control"
                            value={formData.outletId}
                            onChange={handleChange}
                        >
                            {outlets.map(outlet => (
                                <option
                                    key={outlet.outletId}
                                    value={outlet.outletId} // Ensure this is the number value
                                >
                                    {outlet.outletName}
                                </option>
                            ))}
                        </select>
                        {errors.outletId && <div className="error-message">{errors.outletId}</div>}
                    </div>

                    <div className="form-group">
                        <label>Date and Time *</label>
                        <input
                            type="datetime-local"
                            name="reservationDateTime"
                            className="form-control"
                            value={formData.reservationDateTime}
                            onChange={(e) => {
                                setFormData(prev => ({
                                    ...prev,
                                    reservationDateTime: e.target.value
                                }));
                            }}
                            min={new Date().toISOString().slice(0, 16)}
                        />
                        {errors.reservationDateTime && (
                            <div className="error-message">{errors.reservationDateTime}</div>
                        )}
                    </div>

                    <div className="form-group full-width">
                        <label htmlFor="specialRequest">Special Requests (Optional)</label>
                        <textarea
                            id="specialRequest"
                            name="specialRequest"
                            className="form-control"
                            value={formData.specialRequest}
                            onChange={handleChange}
                            rows="3"
                            placeholder="Any special requests or dietary restrictions?"
                        />
                    </div>

                    {errors.submit && <div className="error-message full-width">{errors.submit}</div>}

                    <button
                        type="submit"
                        className="btn-submit"
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? 'Processing...' : 'Make Reservation'}
                    </button>
                </form>
            )}
        </div>
    );
};

export default ReservationPage;