import React, { useState } from 'react';
import axios from 'axios';

function App() {
  const [originalUrl, setOriginalUrl] = useState('');
  const [shortCode, setShortCode] = useState('');

  const handleSubmit = async (e) => {
    e.preventDefault();
    const res = await axios.post('http://localhost:5054/api/url/shorten', originalUrl, {
      headers: { 'Content-Type': 'application/json' }
    });
    setShortCode(res.data.shortCode);
  };

  return (
    <div style={{ padding: '40px' }}>
      <h2>URL Shortener</h2>
      <form onSubmit={handleSubmit}>
        <input
          type="text"
          placeholder="Enter URL"
          value={originalUrl}
          onChange={(e) => setOriginalUrl(e.target.value)}
        />
        <button type="submit">Shorten</button>
      </form>
      {shortCode && (
        <p>
          Shortened URL: <a href={`http://localhost:5054/api/url/${shortCode}`} target="_blank" rel="noreferrer">
            {`http://localhost:5054/api/url/${shortCode}`}
          </a>
        </p>
      )}
    </div>
  );
}

export default App;
