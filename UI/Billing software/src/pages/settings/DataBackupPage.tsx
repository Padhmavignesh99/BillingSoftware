import { useState, useEffect } from 'react';
import { Database, Download, Plus, HardDrive, Clock, CheckCircle, AlertCircle } from 'lucide-react';
import apiClient from '../../services/apiClient';

interface BackupRecord {
  id: number;
  fileName: string;
  fileSize: number;
  status: string;
  backupDate: string;
  notes?: string;
}

const formatBytes = (bytes: number) => {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
};

export default function DataBackupPage() {
  const [backups, setBackups] = useState<BackupRecord[]>([]);
  const [loading, setLoading] = useState(true);
  const [creating, setCreating] = useState(false);

  useEffect(() => { loadBackups(); }, []);

  const loadBackups = async () => {
    setLoading(true);
    try {
      const res = await apiClient.get('/backup');
      setBackups(res.data.data || []);
    } catch { /* ignore */ } finally { setLoading(false); }
  };

  const createBackup = async () => {
    setCreating(true);
    try {
      await apiClient.post('/backup', {});
      loadBackups();
    } catch { /* ignore */ } finally { setCreating(false); }
  };

  const downloadBackup = async (id: number, fileName: string) => {
    try {
      const res = await apiClient.get(`/backup/${id}/download`, { responseType: 'blob' });
      const url = window.URL.createObjectURL(new Blob([res.data]));
      const a = document.createElement('a');
      a.href = url; a.download = fileName;
      a.click(); window.URL.revokeObjectURL(url);
    } catch { /* ignore */ }
  };

  return (
    <div className="p-6 space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Database className="w-7 h-7 text-emerald-600" />
          <div>
            <h1 className="text-xl font-bold text-gray-800">Data Backup</h1>
            <p className="text-xs text-gray-500">Create and download backups of your business data</p>
          </div>
        </div>
        <button onClick={createBackup} disabled={creating}
          className="bg-emerald-600 text-white px-4 py-2 rounded-lg hover:bg-emerald-700 text-sm flex items-center gap-2 disabled:opacity-60">
          <Plus className="w-4 h-4" />
          {creating ? 'Creating...' : 'Create Backup'}
        </button>
      </div>

      {/* Info Card */}
      <div className="bg-emerald-50 border border-emerald-200 rounded-xl p-4 flex gap-3">
        <HardDrive className="w-5 h-5 text-emerald-600 flex-shrink-0 mt-0.5" />
        <div>
          <p className="text-sm font-medium text-emerald-800">What's included in a backup?</p>
          <p className="text-xs text-emerald-700 mt-1">
            Backups include a JSON summary of all your invoices, customers, products, and transaction counts.
            Store backups safely and download them anytime.
          </p>
        </div>
      </div>

      {/* Backup List */}
      <div className="bg-white rounded-xl border border-gray-200 overflow-hidden">
        <div className="p-4 border-b border-gray-100">
          <h2 className="font-semibold text-gray-700">Backup History</h2>
        </div>

        {loading ? (
          <div className="p-8 text-center text-gray-400 text-sm">Loading backups...</div>
        ) : backups.length === 0 ? (
          <div className="p-12 text-center">
            <Database className="w-12 h-12 mx-auto mb-3 text-gray-200" />
            <p className="text-gray-400 text-sm">No backups yet. Create your first backup.</p>
          </div>
        ) : (
          <div className="divide-y divide-gray-50">
            {backups.map(b => (
              <div key={b.id} className="flex items-center gap-4 p-4 hover:bg-gray-50 transition-colors">
                <div className={`w-8 h-8 rounded-full flex items-center justify-center flex-shrink-0
                  ${b.status === 'Completed' ? 'bg-green-100' : 'bg-yellow-100'}`}>
                  {b.status === 'Completed'
                    ? <CheckCircle className="w-4 h-4 text-green-600" />
                    : <AlertCircle className="w-4 h-4 text-yellow-600" />}
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium text-gray-800 truncate">{b.fileName}</p>
                  <div className="flex items-center gap-3 mt-0.5">
                    <span className="text-xs text-gray-400 flex items-center gap-1">
                      <Clock className="w-3 h-3" />
                      {new Date(b.backupDate).toLocaleString()}
                    </span>
                    <span className="text-xs text-gray-400">{formatBytes(b.fileSize)}</span>
                  </div>
                </div>
                <span className={`text-xs px-2 py-0.5 rounded-full ${b.status === 'Completed' ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}`}>
                  {b.status}
                </span>
                <button onClick={() => downloadBackup(b.id, b.fileName)}
                  className="text-emerald-600 hover:text-emerald-800 p-2 rounded-lg hover:bg-emerald-50 transition-colors">
                  <Download className="w-4 h-4" />
                </button>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}

