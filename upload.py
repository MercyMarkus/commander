import gspread
from oauth2client.service_account import ServiceAccountCredentials
import pandas as pd

scope = ["https://spreadsheets.google.com/feeds", 'https://www.googleapis.com/auth/spreadsheets',
         "https://www.googleapis.com/auth/drive.file", "https://www.googleapis.com/auth/drive"]

credentials = ServiceAccountCredentials.from_json_keyfile_name('client_secret.json', scope)
client = gspread.authorize(credentials)

Sheet_name ="Speed"
spreadsheet = client.open(Sheet_name)

with open('speed-results.csv', 'r') as file_obj:
    content = file_obj.read()
    client.import_csv(spreadsheet.id, data=content)


worksheet = spreadsheet.worksheet(Sheet_name)
df = pd.DataFrame(worksheet.get_all_records())

column_names =  {"connectionType":"Connection Type", "datetime":"Date", 
                 "downloadSpeed":"Download Speed (Mbps)", "latency":"Latency (Milliseconds)", "uploadSpeed":"Upload Speed (Mbps)"}

df.rename(columns = column_names, inplace = True)

df_wired = df[df["Connection Type"] == "Wired"]
df_wireless = df[df["Connection Type"] == "Wireless"]

from plotly.subplots import make_subplots

fig = make_subplots(specs=[[{"secondary_y": False}]])

fig.add_scatter(x=df_wired["Date"], y=df_wired["Download Speed (Mbps)"],
                marker_color="orange", name="Download Speed (Mbps) - Wired")

fig.add_scatter(x=df_wired["Date"], y=df_wired["Upload Speed (Mbps)"],
                marker_color="mediumpurple", name="Upload Speed (Mbps) - Wired")

fig.add_scatter(x=df_wireless["Date"], y=df_wireless["Download Speed (Mbps)"],
                marker_color="blue", name="Download Speed (Mbps) - Wireless")

fig.add_scatter(x=df_wireless["Date"], y=df_wireless["Upload Speed (Mbps)"],
                marker_color="green", name="Upload Speed (Mbps) - Wireless")

fig.update_yaxes(title="Speed (Mbps)")
fig.update_xaxes(title="Date")

fig.update_layout(title={ "y":0.9, "x":0.5, "xanchor": "center", "yanchor": "top"})

fig.update_layout(title_text="Speed Test History")

import chart_studio
import chart_studio.plotly as ply

username = '<add-plotly-username>'
api_key = '<add-plotly-password>'
chart_studio.tools.set_credentials_file(username=username, api_key=api_key)


ply.plot(fig, filename="Speed Test History")