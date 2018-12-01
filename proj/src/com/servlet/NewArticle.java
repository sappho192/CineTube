package com.servlet;

import java.sql.*;

import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import javax.servlet.http.HttpSession;

/**
 * Servlet implementation class Signup
 */
@WebServlet("/NewArticle")
public class NewArticle extends HttpServlet {
	private static final long serialVersionUID = 1L;
       
    /**
     * @see HttpServlet#HttpServlet()
     */
    public NewArticle() {
        super();
    }

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		request.setCharacterEncoding("UTF-8");
		response.setCharacterEncoding("UTF-8");
		response.setContentType("text/html; charset=UTF-8");
		
		new_article(request, response);
	}

	protected void new_article(HttpServletRequest request, HttpServletResponse response) {
		try {
			HttpSession session = request.getSession();
			String u_num = (String)session.getAttribute("u_num");
			String title = request.getParameter("title");
			String content = request.getParameter("content");

			PrintWriter out;
			out = response.getWriter();

			// mssql 연결
			Class.forName("com.microsoft.sqlserver.jdbc.SQLServerDriver");
			String connectionUrl = "jdbc:sqlserver://localhost:1433;Database=IP_final;IntegratedSecurity=true";
			Connection conn = DriverManager.getConnection(connectionUrl);
			
			// Query 진행
			PreparedStatement pstmt1 = conn.prepareStatement("SELECT COUNT (*) as cnt from article_data");
			ResultSet rs = pstmt1.executeQuery();
			rs.next();
			int cnt = Integer.parseInt(rs.getString("cnt"));
			
			// Query 진행
			PreparedStatement pstmt2 = conn.prepareStatement("insert into article_data values (?, ?, ?, getdate(), ?)");

			pstmt2.setInt(1, cnt + 1);
			pstmt2.setString(2, title);
			pstmt2.setString(3, content);
			pstmt2.setInt(4, Integer.parseInt(u_num));
			pstmt2.executeUpdate();
			
			// 경고창 띄우고 첫 화면으로 이동
			out.println("<script>");
			out.println("alert('글쓰기가 완료되었습니다.');");
			out.println("location.href = 'Board.jsp'"); 
			out.println("</script>"); 
			out.close();
			
			rs.close();
			pstmt1.close();
			pstmt2.close();
			conn.close();
		} catch (IOException e) {
			e.printStackTrace();
		} catch (ClassNotFoundException e) {
			e.printStackTrace();
		} catch (SQLException e) {
			e.printStackTrace();
		}
	}
}
