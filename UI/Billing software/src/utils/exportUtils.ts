export function exportToCSV(
  filename: string,
  rows: any[],
  headers?: { key: string; label: string }[]
) {
  if (!rows || rows.length === 0) return;

  let csvContent = '';

  if (headers) {
    csvContent += headers.map((h) => `"${h.label}"`).join(',') + '\r\n';
    rows.forEach((row) => {
      const line = headers
        .map((h) => {
          let val = row[h.key];
          if (val === null || val === undefined) val = '';
          val = String(val).replace(/"/g, '""');
          return `"${val}"`;
        })
        .join(',');
      csvContent += line + '\r\n';
    });
  } else {
    const keys = Object.keys(rows[0]);
    csvContent += keys.map((k) => `"${k}"`).join(',') + '\r\n';
    rows.forEach((row) => {
      const line = keys
        .map((k) => {
          let val = row[k];
          if (val === null || val === undefined) val = '';
          val = String(val).replace(/"/g, '""');
          return `"${val}"`;
        })
        .join(',');
      csvContent += line + '\r\n';
    });
  }

  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const link = document.createElement('a');
  link.setAttribute('href', url);
  link.setAttribute('download', `${filename}.csv`);
  document.body.appendChild(link);
  link.click();
  document.body.removeChild(link);
  URL.revokeObjectURL(url);
}
