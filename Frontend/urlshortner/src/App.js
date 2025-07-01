import React, { useState, useRef } from 'react';
import axios from 'axios';
import { QRCodeCanvas } from 'qrcode.react';
import './App.css';

function App() {
  const [originalUrl, setOriginalUrl] = useState('');
  const [shortCode, setShortCode] = useState('');
  const [error, setError] = useState('');
  const qrRef = useRef();

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setShortCode('');
    try {
      const res = await axios.post('http://localhost:5054/api/url/shorten', originalUrl, {
        headers: { 'Content-Type': 'application/json' }
      });
      const code = res.data.shortCode;
      setShortCode(code); // only show the code
    } catch (err) {
      if (err.response && err.response.data) {
        setError(err.response.data);
      } else {
        setError("Something went wrong");
      }
    }
  };

  const downloadQR = () => {
    const canvas = qrRef.current.querySelector('canvas');
    const pngUrl = canvas.toDataURL('image/png');
    const link = document.createElement('a');
    link.href = pngUrl;
    link.download = 'qrcode.png';
    link.click();
  };

  return (
    <div className="container">
      <div className="card">
        <h2>🔗 URL Shortener</h2>
        <form onSubmit={handleSubmit} className="form">
          <input
            type="text"
            placeholder="Enter your long URL..."
            value={originalUrl}
            onChange={(e) => setOriginalUrl(e.target.value)}
            required
          />
          <button type="submit">Shorten</button>
        </form>

        {error && <p style={{ color: 'red', marginTop: '10px' }}>{error}</p>}

        {shortCode && (
          <div className="result">
            <p>
      Short Link:{" "}
      <a
        href={`http://localhost:5054/api/url/${shortCode}`}
        target="_blank"
        rel="noreferrer"
      >
        {shortCode}
      </a>
    </p>
            <div ref={qrRef} className="qr">
              <QRCodeCanvas value={`http://localhost:5054/api/url/${shortCode}`} size={180} />
            </div>
            <button onClick={downloadQR} className="download-btn">Download QR Code</button>
          </div>
        )}
      </div>
    </div>
  );
}

export default App;
