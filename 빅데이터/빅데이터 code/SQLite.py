import sqlite3
import pandas as pd

class SQLite:
    def __init__(self):
        self.conn = sqlite3.connect('D:/2604340036 - 송성민/빅데이터/빅데이터/data/emp.db')
        self.cur = self.conn.cursor()

    def close_db(self):
        self.cur.close()
        self.conn.close()

    def run_sql(self, sql):
        self.cur.execute(sql)
        result = self.cur.fetchall()
        columns = [column[0] for column in self.cur.description]
        df_result = pd.DataFrame(result, columns=columns)

        return df_result