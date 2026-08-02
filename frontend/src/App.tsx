import { useState } from 'react'
import type { FormEvent } from 'react'
import './App.css'

type Status = 'idle' | 'loading' | 'success' | 'error'

function App() {
  const [file, setFile] = useState<File | null>(null)
  const [status, setStatus] = useState<Status>('idle')
  const [summary, setSummary] = useState('')

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (!file) return

    setStatus('loading')

    const formData = new FormData()
    formData.append('file', file)

    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/audio`, {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        setStatus('error')
        return
      }

      const data = await response.json()
      setSummary(data.summary)
      setStatus('success')
    } catch {
      setStatus('error')
    }
  }

  return (
    <div className="App">
      <h1>Subir audio</h1>
      <form onSubmit={handleSubmit}>
        <input
          type="file"
          accept="audio/*"
          onChange={(event) => setFile(event.target.files?.[0] ?? null)}
        />
        <button type="submit" disabled={status === 'loading'}>
          Subir
        </button>
      </form>

      {status === 'loading' && <p>Subiendo archivo...</p>}
      {status === 'success' && <p>Resumen: {summary}</p>}
      {status === 'error' && <p>Ocurrió un error al subir el archivo.</p>}
    </div>
  )
}

export default App
