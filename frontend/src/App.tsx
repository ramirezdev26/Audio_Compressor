import { useState } from 'react'
import type { ChangeEvent, FormEvent } from 'react'
import './App.css'
import type { AudioValidationResult } from './workers/audioValidation'

type Status = 'idle' | 'loading' | 'success' | 'error'

type UploadStatus = 'validating' | 'valid' | 'invalid' | 'uploading' | 'uploaded' | 'error'

interface UploadedAudio {
  id: string
  name: string
  size: number
  status: UploadStatus
  reason?: string
  summary?: string
}

function formatFileSize(bytes: number): string {
  const megabytes = bytes / (1024 * 1024)
  if (megabytes >= 1) {
    return `${megabytes.toFixed(2)} MB`
  }
  return `${(bytes / 1024).toFixed(2)} KB`
}

function describeUploadStatus(upload: UploadedAudio): string {
  switch (upload.status) {
    case 'validating':
      return 'Validando...'
    case 'valid':
      return '✅ Válido'
    case 'invalid':
      return `❌ Inválido: ${upload.reason ?? 'Formato de audio no soportado'}`
    case 'uploading':
      return 'Subiendo...'
    case 'uploaded':
      return '✅ Subido'
    case 'error':
      return '❌ Error'
  }
}

function App() {
  const [file, setFile] = useState<File | null>(null)
  const [status, setStatus] = useState<Status>('idle')
  const [summary, setSummary] = useState('')
  const [uploads, setUploads] = useState<UploadedAudio[]>([])
  const [currentUploadId, setCurrentUploadId] = useState<string | null>(null)

  function updateUpload(id: string, changes: Partial<UploadedAudio>) {
    setUploads((prev) => prev.map((upload) => (upload.id === id ? { ...upload, ...changes } : upload)))
  }

  async function handleFileChange(event: ChangeEvent<HTMLInputElement>) {
    const selected = event.target.files?.[0] ?? null
    setFile(selected)

    if (!selected) {
      setCurrentUploadId(null)
      return
    }

    const id = `${Date.now()}-${Math.random()}`
    setCurrentUploadId(id)
    setUploads((prev) => [
      ...prev,
      { id, name: selected.name, size: selected.size, status: 'validating' },
    ])

    const headerBuffer = await selected.slice(0, 64).arrayBuffer()

    const worker = new Worker(new URL('./workers/audioValidator.worker.ts', import.meta.url), {
      type: 'module',
    })

    worker.onmessage = (workerEvent: MessageEvent<AudioValidationResult>) => {
      const { valid, reason } = workerEvent.data
      updateUpload(id, { status: valid ? 'valid' : 'invalid', reason })
      worker.terminate()
    }

    worker.postMessage({ fileBuffer: headerBuffer, fileSize: selected.size }, [headerBuffer])
  }

  async function handleSubmit(event: FormEvent) {
    event.preventDefault()
    if (!file || !currentUploadId) return

    setStatus('loading')
    updateUpload(currentUploadId, { status: 'uploading' })

    const formData = new FormData()
    formData.append('file', file)

    try {
      const response = await fetch(`${import.meta.env.VITE_API_URL}/api/audio`, {
        method: 'POST',
        body: formData,
      })

      if (!response.ok) {
        setStatus('error')
        updateUpload(currentUploadId, { status: 'error' })
        return
      }

      const data = await response.json()
      setSummary(data.summary)
      setStatus('success')
      updateUpload(currentUploadId, { status: 'uploaded', summary: data.summary })
    } catch {
      setStatus('error')
      updateUpload(currentUploadId, { status: 'error' })
    }
  }

  const currentUpload = uploads.find((upload) => upload.id === currentUploadId)
  const canSubmit = currentUpload?.status === 'valid'

  return (
    <div className="App">
      <h1>Subir audio</h1>
      <form onSubmit={handleSubmit}>
        <input type="file" accept="audio/*" onChange={handleFileChange} />
        <button type="submit" disabled={status === 'loading' || !canSubmit}>
          Subir
        </button>
      </form>

      {status === 'loading' && <p>Subiendo archivo...</p>}
      {status === 'success' && <p>Resumen: {summary}</p>}
      {status === 'error' && <p>Ocurrió un error al subir el archivo.</p>}

      {uploads.length > 0 && (
        <ul>
          {uploads.map((upload) => (
            <li key={upload.id}>
              {upload.name} ({formatFileSize(upload.size)}) — {describeUploadStatus(upload)}
            </li>
          ))}
        </ul>
      )}
    </div>
  )
}

export default App
